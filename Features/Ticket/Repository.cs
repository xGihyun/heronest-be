using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DapperQueryBuilder;
using NanoidDotNet;
using Npgsql;

namespace Heronest.Features.Ticket;

public interface ITicketRepository
{
    Task<Ticket[]> GetMany(GetTicketFilter filter);
    Task<Ticket?> GetByTicketNumber(string ticketNumber);
    Task<Ticket> Create(CreateTicketRequest data);
    Task Update(UpdateTicketRequest data);
}

public class TicketRepository : ITicketRepository
{
    private NpgsqlDataSource dataSource;

    public TicketRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Ticket?> GetByTicketNumber(string ticketNumber)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT
                tickets.ticket_id,
                tickets.ticket_number,
                tickets.created_at AS reserved_at,
                tickets.status,
                jsonb_build_object(
                    'user', jsonb_build_object(
                        'user_id', users.user_id,
                        'first_name', users.first_name,
                        'middle_name', users.middle_name,
                        'last_name', users.last_name
                    ),
                    'event', jsonb_build_object(
                        'event_id', events.event_id,
                        'name', events.name,
                        'start_at', events.start_at,
                        'end_at', events.end_at
                    ),
                    'seat', jsonb_build_object(
                        'seat_id', seats.seat_id,
                        'seat_number', seats.seat_number
                    ),
                    'venue', jsonb_build_object(
                        'venue_id', venues.venue_id,
                        'name', venues.name
                    )
                ) AS reservation_json
            FROM tickets
            JOIN users ON users.user_id = tickets.user_id
            JOIN events ON events.event_id = tickets.event_id
            JOIN seats ON seats.seat_id = tickets.seat_id
            JOIN venues ON venues.venue_id = events.venue_id
            WHERE tickets.ticket_number = {ticketNumber}
            "
        );

        var ticket = await sql.QueryFirstOrDefaultAsync<Ticket>();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        ticket.Reservation =
            JsonSerializer.Deserialize<TicketReservation>(ticket.ReservationJson, options)
            ?? throw new JsonException("Failed to deserialize ticket reservation details.");

        return ticket;
    }

    public async Task<Ticket[]> GetMany(GetTicketFilter filter)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT
                tickets.ticket_id,
                tickets.ticket_number,
                tickets.created_at AS reserved_at,
                tickets.status,
                jsonb_build_object(
                    'user', jsonb_build_object(
                        'user_id', users.user_id,
                        'first_name', users.first_name,
                        'middle_name', users.middle_name,
                        'last_name', users.last_name
                    ),
                    'event', jsonb_build_object(
                        'event_id', events.event_id,
                        'name', events.name,
                        'start_at', events.start_at,
                        'end_at', events.end_at
                    ),
                    'seat', jsonb_build_object(
                        'seat_id', seats.seat_id,
                        'seat_number', seats.seat_number
                    ),
                    'venue', jsonb_build_object(
                        'venue_id', venues.venue_id,
                        'name', venues.name
                    )
                ) AS reservation_json
            FROM tickets
            JOIN users ON users.user_id = tickets.user_id
            JOIN events ON events.event_id = tickets.event_id
            JOIN seats ON seats.seat_id = tickets.seat_id
            JOIN venues ON venues.venue_id = events.venue_id
            WHERE 1=1
            "
        );

        if (filter.EventId.HasValue)
        {
            sql += $"AND tickets.event_id = {filter.EventId.Value}";
        }

        if (filter.UserId.HasValue)
        {
            sql += $"AND tickets.user_id = {filter.UserId.Value}";
        }

        // NOTE: Should probably be a query param as well.
        sql += $"ORDER BY tickets.created_at DESC";

        if (filter.Offset.HasValue && filter.Limit.HasValue)
        {
            sql += $"OFFSET {filter.Offset} LIMIT {filter.Limit}";
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        var tickets = await sql.QueryAsync<Ticket>();
        tickets = tickets.Select(ticket =>
        {
            ticket.Reservation =
                JsonSerializer.Deserialize<TicketReservation>(ticket.ReservationJson, options)
                ?? throw new JsonException("Failed to deserialize ticket reservation details.");

            return ticket;
        });

        return tickets.ToArray();
    }

    // TODO:
    // If `transaction` is `null`, begin its own transaction.
    public async Task<Ticket> Create(CreateTicketRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var ticketNumber = Nanoid.Generate("0123456789ABCDEF", 6);

        var sql = conn.QueryBuilder(
            $@"
            INSERT INTO tickets (user_id, seat_id, event_id, ticket_number, status)
            VALUES (
                {data.UserId}, 
                {data.SeatId}, 
                {data.EventId}, 
                {ticketNumber}, 
                'reserved'
                )
            "
        );

        await sql.ExecuteAsync();

        var ticket = await this.GetByTicketNumber(ticketNumber);

        if (ticket is null)
        {
            throw new Exception("Ticket is null after creation.");
        }

        return ticket;
    }

    public async Task Update(UpdateTicketRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            UPDATE tickets 
            SET status = {data.Status}
            WHERE ticket_id = {data.TicketId}
            "
        );

        await sql.ExecuteAsync();
    }
}
