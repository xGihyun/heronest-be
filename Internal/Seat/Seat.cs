using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Internal.Database;
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
}

[SqlMapper(CaseType.SnakeCase)]
public class GetSeatResponse : CreateSeatRequest;

public interface ISeatRepository
{
    Task Create(CreateSeatRequest[] data);
    Task<GetSeatResponse[]> Get(Guid venueId);
}

public class SeatRepository : ISeatRepository
{
    private NpgsqlConnection conn;

    public SeatRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<GetSeatResponse[]> Get(Guid venueId)
    {
        var sql =
            @"
            SELECT seat_number, status, seat_section_id, venue_id, metadata, seat_id
            FROM seats
            WHERE venue_id = @VenueId
            ";

        var seatsResult = await this.conn.QueryAsync<GetSeatResponse>(
            sql,
            new { VenueId = venueId }
        );

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

                return v;
            })
            .ToArray();

        return seats;
    }

    // TODO: Use transactions
    public async Task Create(CreateSeatRequest[] data)
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
        }
    }
}
