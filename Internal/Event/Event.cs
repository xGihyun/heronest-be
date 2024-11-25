using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Npgsql;
using Heronest.Internal.Database;

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

[SqlMapper(CaseType.SnakeCase)]
public class GetEventResponse 
{
    [Column("event_id")]
    public Guid EventId { get; set; }

    [Column("name")]
    public String Name { get; set; } = String.Empty;

    [Column("description")]
    public String? Description { get; set; }
}

public interface IEventRepository
{
    Task<GetEventResponse[]> Get(int page, int limit);
    Task Create(CreateEventRequest data);
}

public class EventRepository : IEventRepository
{
    private NpgsqlConnection conn;

    public EventRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<GetEventResponse[]> Get(int page, int limit)
    {
        var sql =
            @"
            SELECT event_id, name, description
            FROM events
            OFFSET @Offset
            LIMIT @Limit
            ";

        var events = await this.conn.QueryAsync<GetEventResponse>(
            sql,
            new { Offset = (page - 1) * limit, Limit = limit }
        );

        return events.ToArray();
    }

    public async Task Create(CreateEventRequest data)
    {
        var sql =
            @"
            INSERT INTO events(name, description)
            VALUES(@Name, @Description)
            ";

        await conn.ExecuteAsync(sql, data);
    }
}

