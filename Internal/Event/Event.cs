using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Database;
using Heronest.Internal.Venue;
using Npgsql;

namespace Heronest.Internal.Event;

public class CreateEventRequest
{
    [Column("event_id")]
    public Guid EventId { get; set; }

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

    [Column("image_url")]
    public string? ImageUrl { get; set; }
}

public class UpdateEventRequest : CreateEventRequest;

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

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("venue_json")]
    [JsonIgnore]
    public string VenueJson { get; set; } = string.Empty;

    [Column("venue")]
    public GetVenueResponse Venue { get; set; } = new GetVenueResponse();
}

public class GetEventFilter : PaginationResult
{
    public Guid? VenueId { get; set; }
    public string? Name { get; set; }
}

public interface IEventRepository
{
    Task<GetEventResponse[]> Get(GetEventFilter pagination);
    Task Create(CreateEventRequest data);
    Task Update(UpdateEventRequest data);
}

public class EventRepository : IEventRepository
{
    private NpgsqlDataSource dataSource;

    public EventRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<GetEventResponse[]> Get(GetEventFilter filter)
    {
        var sql =
            @"
            SELECT 
                events.event_id, 
                events.name, 
                events.description, 
                events.start_at, 
                events.end_at,
                events.image_url,
                jsonb_build_object(
                    'venue_id', venues.venue_id,
                    'name', venues.name,
                    'description', venues.description,
                    'capacity', venues.capacity,
                    'location', venues.location,
                    'image_url', venues.image_url
                ) AS venue_json
            FROM events
            JOIN venues ON venues.venue_id = events.venue_id
            ";

        var parameters = new DynamicParameters();

        // NOTE: This is gonna conflict with the `name` filter, but the frontend
        // doesn't need to filter on both `venue_id` and `name` so it's fine for
        // now.
        if (filter.VenueId.HasValue)
        {
            sql += " WHERE venues.venue_id = @VenueId";
            parameters.Add("VenueId", filter.VenueId);
        }

        if (filter.Name is not null)
        {
            sql +=
                @" 
                WHERE events.name ILIKE @Name
                ";
            parameters.Add("Name", $"%{filter.Name}%");
        }

        sql += " ORDER BY events.start_at";

        if (filter.Page.HasValue && filter.Limit.HasValue)
        {
            sql += " OFFSET @Offset LIMIT @Limit";
            parameters.Add("Offset", (filter.Page.Value - 1) * filter.Limit.Value);
            parameters.Add("Limit", filter.Limit.Value);
        }


        await using var conn = await this.dataSource.OpenConnectionAsync();
        var eventsResult = await conn.QueryAsync<GetEventResponse>(sql, parameters);
        var events = eventsResult
            .Select(v =>
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                };

                var venue = JsonSerializer.Deserialize<GetVenueResponse>(v.VenueJson, options);

                if (venue is null)
                {
                    throw new Exception("Venue is null.");
                }

                v.Venue = venue;

                return v;
            })
            .ToArray();

        return events;
    }

    public async Task Create(CreateEventRequest data)
    {
        var sql =
            @"
            INSERT INTO events (name, description, start_at, end_at, venue_id, image_url)
            VALUES (@Name, @Description, @StartAt, @EndAt, @VenueId, @ImageUrl)
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }

    public async Task Update(UpdateEventRequest data)
    {
        var sql =
            @"
            UPDATE events 
            SET name = @Name,
                description = @Description, 
                start_at = @StartAt, 
                end_at = @EndAt, 
                venue_id = @VenueId,
                image_url = @ImageUrl
            WHERE event_id = @EventId
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }
}
