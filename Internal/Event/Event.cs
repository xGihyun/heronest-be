using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Npgsql;

namespace Heronest.Internal.Event;

public class CreateEventRequest
{
    [Column("name")]
    public String Name { get; set; } = String.Empty;

    [Column("description")]
    public String? Description { get; set; }

    [Column("start_at")]
    public DateTime StartAt { get; set; }

    [Column("end_at")]
    public DateTime EndAt { get; set; }

    [Column("venue_id")]
    public Guid VenueId { get; set; }
}

public interface IEventRepository
{
    Task Create(CreateEventRequest data);
}

public class EventRepository : IEventRepository
{
    private NpgsqlConnection conn;

    public EventRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Create(CreateEventRequest data)
    {
        var sql =
            @"
            INSERT INTO events(name, description)
            VALUES(@Name, @Description)

            INSERT INTO event_occurrences(start_at, end_at, venue_id)
            VALUES(@StartAt, @EndAt, @VenueId)
            ";

        await conn.ExecuteAsync(sql, data);
    }
}

