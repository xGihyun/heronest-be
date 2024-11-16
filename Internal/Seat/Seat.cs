
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper; 
using Npgsql;

namespace Heronest.Internal.Seat;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status 
{
    Reserved, 
    Available, 
    Unavailable
}

public interface ISeatRepository
{
    Task Create(CreateSeatRequest data);
}

public class CreateSeatRequest
{
    [Column("seat_number")]
    public string SeatNumber {get; set;} = String.Empty;

    [Column("status")]
    public Status SeatStatus {get; set;}

    [Column("seat_section_id")]
    public Guid SeatSectionId {get; set;}

    [Column("venue_id")]
    public Guid VenueId {get; set;}
}

public class SeatRepository : ISeatRepository
{
    private NpgsqlConnection conn; 
    
    public SeatRepository(NpgsqlConnection conn)
    {
        this.conn = conn; 
    }

    public async Task Create(CreateSeatRequest data)
    {
        var sql = 
        @"
        INSERT INTO seats(seat_number, status, seat_section_id, venue_id)
        VALUES(@SeatNumber, @SeatStatus, @SeatSectionId, @VenueId)
        ";
            
        await conn.ExecuteAsync(sql);
    }
}
