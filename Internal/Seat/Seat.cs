using System.ComponentModel.DataAnnotations.Schema;
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
    [Column("seat_number")]
    public string SeatNumber { get; set; } = String.Empty;

    [Column("status")]
    public SeatStatus Status { get; set; }

    [Column("seat_section_id")]
    public Guid? SeatSectionId { get; set; }

    [Column("venue_id")]
    public Guid VenueId { get; set; }

    [Column("metadata")]
    public object Metadata { get; set; } = new object { };
}

[SqlMapper(CaseType.SnakeCase)]
public class GetSeatResponse : CreateSeatRequest
{
    [Column("seat_id")]
    public Guid SeatId { get; set; }
}

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

        var seats = await this.conn.QueryAsync<GetSeatResponse>(sql, new { VenueId = venueId });

        return seats.ToArray();
    }

    // TODO: Use transactions
    public async Task Create(CreateSeatRequest[] data)
    {
        var sql =
            @"
            INSERT INTO seats (seat_number, status, seat_section_id, venue_id, metadata)
            VALUES (@SeatNumber, @Status::seat_status, @SeatSectionId, @VenueId, @Metadata)
            ";

        foreach (var seat in data)
        {
            await conn.ExecuteAsync(
                sql,
                new
                {
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
