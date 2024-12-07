using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Database;
using Heronest.Internal.Seat;
using Heronest.Internal.User;
using NanoidDotNet;
using Npgsql;

namespace Heronest.Internal.Ticket;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TicketStatus
{
    Reserved,
    Used,
    Canceled,
}

public class CreateTicketRequest
{
    [Column("metadata")]
    public dynamic? Metadata { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("seat_id")]
    public Guid SeatId { get; set; }

    [Column("event_id")]
    public Guid EventId { get; set; }
}

public class CreateTicketResponse : GetTicketResponse;

public class GetTicketFilter
{
    public Guid? EventId { get; set; }
}

/*public class CreateTicketResponse*/
/*{*/
/*    [Column("ticket_id")]*/
/*    public Guid TicketId { get; set; }*/
/**/
/*    [Column("ticket_number")]*/
/*    public string TicketNumber { get; set; } = string.Empty;*/
/*}*/

public class SeatDetail
{
    [Column("seat_id")]
    public Guid SeatId { get; set; }

    [Column("seat_number")]
    public string SeatNumber { get; set; } = String.Empty;
}

public class VenueDetail
{
    [Column("venue_id")]
    public Guid VenueId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}

[SqlMapper(CaseType.SnakeCase)]
public class GetTicketResponse
{
    [Column("ticket_id")]
    public Guid TicketId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("ticket_number")]
    public string TicketNumber { get; set; } = string.Empty;

    [Column("status")]
    public TicketStatus Status { get; set; }

    [Column("metadata")]
    public dynamic? Metadata { get; set; }

    [Column("user_json")]
    [JsonIgnore]
    public string UserJson { get; set; } = string.Empty;

    [Column("user")]
    public Person User { get; set; }

    [Column("event_json")]
    [JsonIgnore]
    public string EventJson { get; set; } = string.Empty;

    [Column("event")]
    public ReservedSeatEventDetail Event { get; set; } = new ReservedSeatEventDetail();

    [Column("seat_json")]
    [JsonIgnore]
    public string SeatJson { get; set; } = string.Empty;

    [Column("seat")]
    public SeatDetail Seat { get; set; } = new SeatDetail();

    [Column("venue_json")]
    [JsonIgnore]
    public string VenueJson { get; set; } = string.Empty;

    [Column("venue")]
    public VenueDetail Venue { get; set; } = new VenueDetail();
}

public class UpdateTicketRequest
{
    [Column("ticket_id")]
    public Guid TicketId { get; set; }

    [Column("status")]
    public TicketStatus Status { get; set; }
}

public interface ITicketRepository
{
    Task<GetTicketResponse[]> Get(GetTicketFilter filter);
    Task<GetTicketResponse> GetByTicketNumber(string ticketNumber);
    Task<GetTicketResponse[]> GetByUserId(Guid userId);
    Task<GetTicketResponse> GetByTicketId(
        Guid ticketId,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    );
    Task<GetTicketResponse> Create(
        CreateTicketRequest data,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    );
    Task Update(UpdateTicketRequest data);
}

public class TicketRepository : ITicketRepository
{
    private NpgsqlDataSource dataSource;

    public TicketRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<GetTicketResponse> GetByTicketNumber(string ticketNumber)
    {
        var sql =
            @"
            SELECT
                tickets.ticket_id,
                tickets.created_at,
                tickets.ticket_number,
                tickets.status,
                tickets.metadata,
                jsonb_build_object(
                    'user_id', users.user_id,
                    'first_name', user_details.first_name,
                    'middle_name', user_details.middle_name,
                    'last_name', user_details.last_name,
                    'birth_date', user_details.birth_date,
                    'sex', user_details.sex
                ) AS user_json,
                jsonb_build_object(
                    'event_id', events.event_id,
                    'name', events.name,
                    'start_at', events.start_at,
                    'end_at', events.end_at
                ) AS event_json,
                jsonb_build_object(
                    'seat_id', seats.seat_id,
                    'seat_number', seats.seat_number
                ) AS seat_json,
                jsonb_build_object(
                    'venue_id', venues.venue_id,
                    'name', venues.name
                ) AS venue_json
            FROM tickets
            JOIN users ON users.user_id = tickets.user_id
            JOIN user_details ON user_details.user_id = users.user_id
            JOIN events ON events.event_id = tickets.event_id
            JOIN seats ON seats.seat_id = tickets.seat_id
            JOIN venues ON venues.venue_id = events.venue_id
            WHERE tickets.ticket_number = @TicketNumber
            ORDER BY tickets.created_at DESC
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        var ticket = await conn.QueryFirstOrDefaultAsync<GetTicketResponse>(
            sql,
            new { TicketNumber = ticketNumber }
        );

        if (ticket is null)
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        ticket.Venue =
            JsonSerializer.Deserialize<VenueDetail>(ticket.VenueJson, options)
            ?? throw new Exception("Ticket venue is null.");

        ticket.User =
            JsonSerializer.Deserialize<Person>(ticket.UserJson, options)
            ?? throw new Exception("Ticket user is null.");

        ticket.Seat =
            JsonSerializer.Deserialize<SeatDetail>(ticket.SeatJson, options)
            ?? throw new Exception("Ticket seat is null.");

        ticket.Event =
            JsonSerializer.Deserialize<ReservedSeatEventDetail>(ticket.EventJson, options)
            ?? throw new Exception("Ticket event is null.");

        return ticket;
    }

    public async Task<GetTicketResponse[]> Get(GetTicketFilter filter)
    {
        return new GetTicketResponse[]{};
        /*var sql =*/
        /*    @"*/
        /*    SELECT*/
        /*        tickets.ticket_id,*/
        /*        tickets.created_at,*/
        /*        tickets.ticket_number,*/
        /*        tickets.status,*/
        /*        tickets.metadata,*/
        /*        jsonb_build_object(*/
        /*            'user_id', users.user_id,*/
        /*            'first_name', user_details.first_name,*/
        /*            'middle_name', user_details.middle_name,*/
        /*            'last_name', user_details.last_name,*/
        /*            'birth_date', user_details.birth_date,*/
        /*            'sex', user_details.sex*/
        /*        ) AS user_json,*/
        /*        jsonb_build_object(*/
        /*            'event_id', events.event_id,*/
        /*            'name', events.name,*/
        /*            'start_at', events.start_at,*/
        /*            'end_at', events.end_at*/
        /*        ) AS event_json,*/
        /*        jsonb_build_object(*/
        /*            'seat_id', seats.seat_id,*/
        /*            'seat_number', seats.seat_number*/
        /*        ) AS seat_json,*/
        /*        jsonb_build_object(*/
        /*            'venue_id', venues.venue_id,*/
        /*            'name', venues.name*/
        /*        ) AS venue_json*/
        /*    FROM tickets*/
        /*    JOIN users ON users.user_id = tickets.user_id*/
        /*    JOIN user_details ON user_details.user_id = users.user_id*/
        /*    JOIN events ON events.event_id = tickets.event_id*/
        /*    JOIN seats ON seats.seat_id = tickets.seat_id*/
        /*    JOIN venues ON venues.venue_id = events.venue_id*/
        /*    ";*/
        /**/
        /*var parameters = new DynamicParameters();*/
        /**/
        /*if (filter.EventId.HasValue)*/
        /*{*/
        /*    sql += " WHERE tickets.event_id = @EventId";*/
        /*    parameters.Add("EventId", filter.EventId.Value);*/
        /*}*/
        /**/
        /*sql += " ORDER BY tickets.created_at DESC";*/
        /**/
        /*if (filter.Page.HasValue && filter.Limit.HasValue)*/
        /*{*/
        /*    sql += " OFFSET @Offset LIMIT @Limit";*/
        /*    parameters.Add("Offset", (filter.Page.Value - 1) * filter.Limit.Value);*/
        /*    parameters.Add("Limit", filter.Limit.Value);*/
        /*}*/
        /**/
        /*await using var conn = await this.dataSource.OpenConnectionAsync();*/
        /*var ticketsResult = await conn.QueryAsync<GetTicketResponse>(sql, parameters);*/
        /*var tickets = ticketsResult.Select(v =>*/
        /*{*/
        /*    var options = new JsonSerializerOptions*/
        /*    {*/
        /*        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,*/
        /*    };*/
        /**/
        /*    var venue = JsonSerializer.Deserialize<VenueDetail>(v.VenueJson, options);*/
        /**/
        /*    if (venue is null)*/
        /*    {*/
        /*        throw new Exception("Ticket venue is null.");*/
        /*    }*/
        /**/
        /*    v.Venue = venue;*/
        /**/
        /*    var user = JsonSerializer.Deserialize<UserDetailRequest>(v.UserJson, options);*/
        /**/
        /*    if (user is null)*/
        /*    {*/
        /*        throw new Exception("Ticket user is null.");*/
        /*    }*/
        /**/
        /*    v.User = user;*/
        /**/
        /*    var seat = JsonSerializer.Deserialize<SeatDetail>(v.SeatJson, options);*/
        /**/
        /*    if (seat is null)*/
        /*    {*/
        /*        throw new Exception("Ticket seat is null.");*/
        /*    }*/
        /**/
        /*    v.Seat = seat;*/
        /**/
        /*    var ticketEvent = JsonSerializer.Deserialize<ReservedSeatEventDetail>(*/
        /*        v.EventJson,*/
        /*        options*/
        /*    );*/
        /**/
        /*    if (ticketEvent is null)*/
        /*    {*/
        /*        throw new Exception("Ticket event is null.");*/
        /*    }*/
        /**/
        /*    v.Event = ticketEvent;*/
        /**/
        /*    return v;*/
        /*});*/
        /**/
        /*return tickets.ToArray();*/
    }

    public async Task<GetTicketResponse[]> GetByUserId(Guid userId)
    {
        return new GetTicketResponse[]{};

        /*// TODO: Put this in a SQL file since it's similar to the query of*/
        /*// `Get()`*/
        /*var sql =*/
        /*    @"*/
        /*    SELECT*/
        /*        tickets.ticket_id,*/
        /*        tickets.created_at,*/
        /*        tickets.ticket_number,*/
        /*        tickets.status,*/
        /*        tickets.metadata,*/
        /*        jsonb_build_object(*/
        /*            'user_id', users.user_id,*/
        /*            'first_name', user_details.first_name,*/
        /*            'middle_name', user_details.middle_name,*/
        /*            'last_name', user_details.last_name,*/
        /*            'birth_date', user_details.birth_date,*/
        /*            'sex', user_details.sex*/
        /*        ) AS user_json,*/
        /*        jsonb_build_object(*/
        /*            'event_id', events.event_id,*/
        /*            'name', events.name,*/
        /*            'start_at', events.start_at,*/
        /*            'end_at', events.end_at*/
        /*        ) AS event_json,*/
        /*        jsonb_build_object(*/
        /*            'seat_id', seats.seat_id,*/
        /*            'seat_number', seats.seat_number*/
        /*        ) AS seat_json,*/
        /*        jsonb_build_object(*/
        /*            'venue_id', venues.venue_id,*/
        /*            'name', venues.name*/
        /*        ) AS venue_json*/
        /*    FROM tickets*/
        /*    JOIN users ON users.user_id = tickets.user_id*/
        /*    JOIN user_details ON user_details.user_id = users.user_id*/
        /*    JOIN events ON events.event_id = tickets.event_id*/
        /*    JOIN seats ON seats.seat_id = tickets.seat_id*/
        /*    JOIN venues ON venues.venue_id = events.venue_id*/
        /*    WHERE tickets.user_id = @UserId*/
        /*    ORDER BY tickets.created_at DESC*/
        /*    ";*/
        /**/
        /*await using var conn = await this.dataSource.OpenConnectionAsync();*/
        /*var ticketsResult = await conn.QueryAsync<GetTicketResponse>(sql, new { UserId = userId });*/
        /*var tickets = ticketsResult.Select(v =>*/
        /*{*/
        /*    var options = new JsonSerializerOptions*/
        /*    {*/
        /*        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,*/
        /*    };*/
        /**/
        /*    var venue = JsonSerializer.Deserialize<VenueDetail>(v.VenueJson, options);*/
        /**/
        /*    if (venue is null)*/
        /*    {*/
        /*        throw new Exception("Ticket venue is null.");*/
        /*    }*/
        /**/
        /*    v.Venue = venue;*/
        /**/
        /*    var user = JsonSerializer.Deserialize<UserDetailRequest>(v.UserJson, options);*/
        /**/
        /*    if (user is null)*/
        /*    {*/
        /*        throw new Exception("Ticket user is null.");*/
        /*    }*/
        /**/
        /*    v.User = user;*/
        /**/
        /*    var seat = JsonSerializer.Deserialize<SeatDetail>(v.SeatJson, options);*/
        /**/
        /*    if (seat is null)*/
        /*    {*/
        /*        throw new Exception("Ticket seat is null.");*/
        /*    }*/
        /**/
        /*    v.Seat = seat;*/
        /**/
        /*    var ticketEvent = JsonSerializer.Deserialize<ReservedSeatEventDetail>(*/
        /*        v.EventJson,*/
        /*        options*/
        /*    );*/
        /**/
        /*    if (ticketEvent is null)*/
        /*    {*/
        /*        throw new Exception("Ticket event is null.");*/
        /*    }*/
        /**/
        /*    v.Event = ticketEvent;*/
        /**/
        /*    return v;*/
        /*});*/
        /**/
        /*return tickets.ToArray();*/
    }

    public async Task<GetTicketResponse?> GetByTicketId(
        Guid ticketId,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    )
    {
        var shouldDisposeConnection = connection == null;
        connection ??= await this.dataSource.OpenConnectionAsync();

        var sql =
            @"
        SELECT
            tickets.ticket_id,
            tickets.created_at,
            tickets.ticket_number,
            tickets.status,
            tickets.metadata,
            jsonb_build_object(
                'user_id', users.user_id,
                'first_name', user_details.first_name,
                'middle_name', user_details.middle_name,
                'last_name', user_details.last_name,
                'birth_date', user_details.birth_date,
                'sex', user_details.sex
            ) AS user_json,
            jsonb_build_object(
                'event_id', events.event_id,
                'name', events.name,
                'start_at', events.start_at,
                'end_at', events.end_at
            ) AS event_json,
            jsonb_build_object(
                'seat_id', seats.seat_id,
                'seat_number', seats.seat_number
            ) AS seat_json,
            jsonb_build_object(
                'venue_id', venues.venue_id,
                'name', venues.name
            ) AS venue_json
        FROM tickets
        JOIN users ON users.user_id = tickets.user_id
        JOIN user_details ON user_details.user_id = users.user_id
        JOIN events ON events.event_id = tickets.event_id
        JOIN seats ON seats.seat_id = tickets.seat_id
        JOIN venues ON venues.venue_id = events.venue_id
        WHERE tickets.ticket_id = @TicketId
        ";

        var ticket = await connection.QueryFirstOrDefaultAsync<GetTicketResponse>(
            sql,
            new { TicketId = ticketId },
            transaction: transaction
        );

        if (ticket is null)
        {
            return null;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        ticket.Venue =
            JsonSerializer.Deserialize<VenueDetail>(ticket.VenueJson, options)
            ?? throw new Exception("Ticket venue is null.");

        ticket.User =
            JsonSerializer.Deserialize<Person>(ticket.UserJson, options)
            ?? throw new Exception("Ticket user is null.");

        ticket.Seat =
            JsonSerializer.Deserialize<SeatDetail>(ticket.SeatJson, options)
            ?? throw new Exception("Ticket seat is null.");

        ticket.Event =
            JsonSerializer.Deserialize<ReservedSeatEventDetail>(ticket.EventJson, options)
            ?? throw new Exception("Ticket event is null.");

        if (shouldDisposeConnection)
        {
            await connection.DisposeAsync();
        }

        return ticket;
    }

    public async Task<GetTicketResponse> Create(
        CreateTicketRequest data,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    )
    {
        var shouldDisposeConnection = connection == null;
        connection ??= await this.dataSource.OpenConnectionAsync();

        try
        {
            var ticketNumber = Nanoid.Generate("0123456789ABCDEF", 6);

            var sql =
                @"
                INSERT INTO tickets (metadata, user_id, seat_id, event_id, ticket_number, status)
                VALUES (@Metadata, @UserId, @SeatId, @EventId, @TicketNumber, 'reserved')
                RETURNING ticket_id
                ";

            var ticketId = await connection.QuerySingleAsync<Guid>(
                sql,
                new
                {
                    Metadata = data.Metadata,
                    UserId = data.UserId,
                    SeatId = data.SeatId,
                    EventId = data.EventId,
                    TicketNumber = ticketNumber,
                },
                transaction: transaction
            );

            var ticket = await this.GetByTicketId(ticketId, connection, transaction);

            return ticket;
        }
        finally
        {
            if (shouldDisposeConnection)
            {
                await connection.DisposeAsync();
            }
        }
    }

    public async Task Update(UpdateTicketRequest data)
    {
        var sql =
            @"
            UPDATE tickets 
            SET status = @Status
            WHERE ticket_id = @TicketId
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }
}
