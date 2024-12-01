using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Internal.Database;
using Heronest.Internal.Ticket;
using Heronest.Internal.User;
using Npgsql;

namespace Heronest.Internal.Seat;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeatStatus
{
    Reserved,
    Available,
    Unavailable,
}

public class CreateSeatRequest
{
    [Column("seat_id")]
    public Guid SeatId { get; set; }

    [Column("seat_number")]
    public string SeatNumber { get; set; } = String.Empty;

    [Column("status")]
    public SeatStatus Status { get; set; }

    [Column("seat_section_id")]
    public Guid? SeatSectionId { get; set; }

    [Column("venue_id")]
    public Guid VenueId { get; set; }

    [Column("metadata")]
    public dynamic Metadata { get; set; } = new object { };

    [Column("reserved_by")]
    public CreateTicketRequest? ReservedBy { get; set; }
}

public class ReservedSeatEventDetail
{
    [Column("event_id")]
    public Guid EventId { get; set; }

    [Column("name")]
    public String Name { get; set; } = String.Empty;
}

public class SeatReservedBy
{
    [Column("user_json")]
    [JsonIgnore]
    public string UserJson { get; set; } = string.Empty;

    [Column("user")]
    public UserDetailRequest User { get; set; } = new UserDetailRequest();

    [Column("event_json")]
    [JsonIgnore]
    public string EventJson { get; set; } = string.Empty;

    [Column("event")]
    public ReservedSeatEventDetail Event { get; set; } = new ReservedSeatEventDetail();
}

[SqlMapper(CaseType.SnakeCase)]
public class GetSeatResponse : CreateSeatRequest
{
    [Column("reserved_by_json")]
    [JsonIgnore]
    public string? ReservedByJson { get; set; }

    [Column("reserved_by")]
    public SeatReservedBy? ReservedBy { get; set; }
};

public interface ISeatRepository
{
    Task Create(CreateSeatRequest data);
    Task CreateMany(CreateSeatRequest[] data);
    Task<GetSeatResponse[]> Get(Guid venueId);
}

public class SeatRepository : ISeatRepository
{
    private NpgsqlDataSource dataSource;

    public SeatRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<GetSeatResponse[]> Get(Guid venueId)
    {
        var sql =
            @"
            SELECT 
                seats.seat_number, 
                seats.status, 
                seats.seat_section_id, 
                seats.venue_id, 
                seats.metadata, 
                seats.seat_id,
                CASE 
                    WHEN users.user_id IS NOT NULL THEN 
                        jsonb_build_object (
                            'user', jsonb_build_object(
                                'user_id', users.user_id,
                                'first_name', user_details.first_name,
                                'middle_name', user_details.middle_name,
                                'last_name', user_details.last_name,
                                'birth_date', user_details.birth_date,
                                'sex', user_details.sex
                            ),
                            'event', jsonb_build_object(
                                'event_id', events.event_id,
                                'name', events.name
                            )
                        )
                    ELSE NULL
                END AS reserved_by_json
            FROM seats
            LEFT JOIN tickets ON tickets.seat_id = seats.seat_id 
            LEFT JOIN users ON users.user_id = tickets.user_id
            LEFT JOIN user_details ON user_details.user_id = users.user_id
            LEFT JOIN events ON events.event_id = tickets.event_id
            WHERE seats.venue_id = @VenueId
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        var seatsResult = await conn.QueryAsync<GetSeatResponse>(sql, new { VenueId = venueId });

        var seats = seatsResult
            .Select(v =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                };

                var metadata = JsonSerializer.Deserialize<dynamic>(v.Metadata, options);

                if (metadata is null)
                {
                    throw new Exception("Failed to deserialize seat metadata.");
                }

                v.Metadata = metadata;

                if (v.ReservedByJson is not null)
                {
                    var reservedBy = JsonSerializer.Deserialize<SeatReservedBy>(
                        v.ReservedByJson,
                        options
                    );

                    if (reservedBy is null)
                    {
                        throw new Exception("Failed to deserialize reserved user.");
                    }

                    v.ReservedBy = reservedBy;
                }

                return v;
            })
            .ToArray();

        return seats;
    }

    public async Task Create(CreateSeatRequest data)
    {
        var sql =
            @"
            INSERT INTO seats (seat_id, seat_number, status, seat_section_id, venue_id, metadata)
            VALUES (@SeatId, @SeatNumber, @Status::seat_status, @SeatSectionId, @VenueId, @Metadata)
            ON CONFLICT(seat_id)
            DO UPDATE SET
                seat_number = @SeatNumber,
                status = @Status::seat_status,
                seat_section_id = @SeatSectionId,
                metadata = @Metadata
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();

        await conn.ExecuteAsync(
            sql,
            new
            {
                SeatId = data.SeatId,
                SeatNumber = data.SeatNumber,
                Status = data.Status.ToString().ToLower(),
                SeatSectionId = data.SeatSectionId,
                VenueId = data.VenueId,
                Metadata = data.Metadata,
            }
        );

        var ticketRepo = new TicketRepository(this.dataSource);

        if (data.ReservedBy is not null)
        {
            await ticketRepo.Create(data.ReservedBy);
        }
    }

    public async Task CreateMany(CreateSeatRequest[] data)
    {
        var sql =
            @"
            INSERT INTO seats (seat_id, seat_number, status, seat_section_id, venue_id, metadata)
            VALUES (@SeatId, @SeatNumber, @Status::seat_status, @SeatSectionId, @VenueId, @Metadata)
            ON CONFLICT(seat_id)
            DO UPDATE SET
                seat_number = @SeatNumber,
                status = @Status::seat_status,
                seat_section_id = @SeatSectionId,
                metadata = @Metadata
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await using var txn = await conn.BeginTransactionAsync();

        var ticketRepo = new TicketRepository(this.dataSource);

        foreach (var seat in data)
        {
            await conn.ExecuteAsync(
                sql,
                new
                {
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    Status = seat.Status.ToString().ToLower(),
                    SeatSectionId = seat.SeatSectionId,
                    VenueId = seat.VenueId,
                    Metadata = seat.Metadata,
                }
            );

            if (seat.ReservedBy is not null)
            {
                await ticketRepo.Create(seat.ReservedBy);
            }
        }

        await txn.CommitAsync();
    }
}
