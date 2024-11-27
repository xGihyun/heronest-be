using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Database;
using Heronest.Internal.Venue;
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

[SqlMapper(CaseType.SnakeCase)]
public class GetEventResponse
{
    [Column("name")]
    public String Name { get; set; } = String.Empty;

    [Column("description")]
    public String? Description { get; set; }

    [Column("start_at")]
    public DateTime StartAt { get; set; }

    [Column("end_at")]
    public DateTime EndAt { get; set; }

    [Column("event_id")]
    public Guid EventId { get; set; }

    [Column("venue")]
    public GetVenueResponse Venue { get; set; } = new GetVenueResponse();
}

public interface IEventRepository
{
    Task<GetEventResponse[]> Get(PaginationResult pagination);
    Task Create(CreateEventRequest data);
}

public class EventRepository : IEventRepository
{
    private NpgsqlConnection conn;

    public EventRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<GetEventResponse[]> Get(PaginationResult pagination)
    {
        var sql =
            @"
            SELECT 
                event_id, name, description, start_at, end_at,
                (
                    json_build_object(
                        'venue_id', venues.venue_id,
                        'name', venues.name,
                        'description', venues.description,
                        'capacity', venues.capacity,
                        'location', venues.location,
                        'image_url', venues.image_url
                    )
                ) AS venue
            FROM events
            JOIN venues ON venues.venue_id = events.venue_id
            ";

        var parameters = new DynamicParameters();

        if (pagination.Page.HasValue && pagination.Limit.HasValue)
        {
            sql += " OFFSET @Offset LIMIT @Limit";
            parameters.Add("Offset", (pagination.Page.Value - 1) * pagination.Limit.Value);
            parameters.Add("Limit", pagination.Limit.Value);
        }

        var events = await this.conn.QueryAsync<GetEventResponse>(sql, parameters);

        return events.ToArray();
    }

    public async Task Create(CreateEventRequest data)
    {
        var sql =
            @"
            INSERT INTO events (name, description, start_at, end_at, venue_id)
            VALUES (@Name, @Description, @StartAt, @EndAt, @VenueId)
            ";

        await conn.ExecuteAsync(sql, data);
    }

    /*public async Task<GetEventOccurrenceResponse[]> GetOccurrences(PaginationResult pagination)*/
    /*{*/
    /*    var sql =*/
    /*        @"*/
    /*        SELECT */
    /*            (*/
    /*                json_build_object(*/
    /*                    'event_id', events.event_id,*/
    /*                    'name', events.name,*/
    /*                    'description', events.description,*/
    /*                    'start_at', event_occurrences.start_at,*/
    /*                    'end_at', event_occurrences.end_at*/
    /*                )*/
    /*            ) AS event,*/
    /*            (*/
    /*                json_build_object(*/
    /*                    'venue_id', venues.venue_id,*/
    /*                    'name', venues.name,*/
    /*                    'description', venues.description,*/
    /*                    'capacity', venues.capacity,*/
    /*                    'location', venues.location,*/
    /*                    'image_url', venues.image_url*/
    /*                )*/
    /*            ) AS venue*/
    /*        FROM event_occurrences*/
    /*        JOIN events ON event_occurrences.event_id = events.event_id*/
    /*        JOIN venues ON event_occurrences.venue_id = venues.venue_id*/
    /*        ";*/
    /**/
    /*    var parameters = new DynamicParameters();*/
    /**/
    /*    if (pagination.Page.HasValue && pagination.Limit.HasValue)*/
    /*    {*/
    /*        sql += " OFFSET @Offset LIMIT @Limit";*/
    /*        parameters.Add("Offset", (pagination.Page.Value - 1) * pagination.Limit.Value);*/
    /*        parameters.Add("Limit", pagination.Limit.Value);*/
    /*    }*/
    /**/
    /*    var occurrences = await this.conn.QueryAsync<GetEventOccurrenceResponse>(sql, parameters);*/
    /**/
    /*    return occurrences.ToArray();*/
    /*}*/
    /**/
    /*public async Task CreateOccurrence(CreateEventOccurrenceRequest data)*/
    /*{*/
    /*    var sql =*/
    /*        @"*/
    /*        INSERT INTO event_occurrences (start_at, end_at, event_id, venue_id)*/
    /*        VALUES (@StartAt, @EndAt, @EventId, @VenueId)*/
    /*        ";*/
    /**/
    /*    await conn.ExecuteAsync(sql, data);*/
    /*}*/
}
