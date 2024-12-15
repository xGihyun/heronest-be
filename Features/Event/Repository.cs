using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DapperQueryBuilder;
using Npgsql;

namespace Heronest.Features.Event;

using Venue = Heronest.Features.Venue.Venue;

public interface IEventRepository
{
    Task<Event[]> GetMany(GetEventFilter filter);
    Task Create(CreateEventRequest data);
    Task Update(CreateEventRequest data);
}

public class EventRepository : IEventRepository
{
    private NpgsqlDataSource dataSource;

    public EventRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Event[]> GetMany(GetEventFilter filter)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            WITH venue_capacity AS (
                SELECT 
                    venue_id,
                    COUNT(seat_id) AS capacity
                FROM seats
                GROUP BY venue_id
            ),
            event_reservations AS (
                SELECT 
                    event_id,
                    COUNT(ticket_id) AS total_reservation
                FROM tickets
                GROUP BY event_id
            )
            SELECT 
                events.event_id, 
                events.name, 
                events.description, 
                events.start_at, 
                events.end_at,
                events.image_url,
                events.allow_visitors,
                jsonb_build_object(
                    'venue_id', venues.venue_id,
                    'name', venues.name,
                    'description', venues.description,
                    'location', venues.location,
                    'image_url', venues.image_url,
                    'capacity', COALESCE(vc.capacity, 0)
                ) AS venue_json,
                COALESCE(er.total_reservation, 0) AS total_reservation
            FROM events
            JOIN venues ON venues.venue_id = events.venue_id
            LEFT JOIN venue_capacity vc ON vc.venue_id = venues.venue_id
            LEFT JOIN event_reservations er ON er.event_id = events.event_id
            WHERE 1=1
            "
        );

        if (filter.VenueId.HasValue)
        {
            sql += $"AND venues.venue_id = {filter.VenueId.Value}";
        }

        if (filter.Name is not null)
        {
            sql += $@"AND events.name ILIKE {$"%{filter.Name}%"}";
        }

        sql += $"GROUP BY events.event_id, venues.venue_id, vc.capacity, er.total_reservation";
        sql += $"ORDER BY events.start_at";

        if (filter.Offset.HasValue && filter.Limit.HasValue)
        {
            sql += $"OFFSET {filter.Offset.Value} LIMIT {filter.Limit.Value}";
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        var events = await sql.QueryAsync<Event>();
        events = events.Select(v =>
        {
            v.Venue =
                JsonSerializer.Deserialize<Venue>(v.VenueJson, options)
                ?? throw new JsonException("Failed to deserialize event venue.");

            return v;
        });

        return events.ToArray();
    }

    public async Task Create(CreateEventRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            INSERT INTO events (name, description, start_at, end_at, venue_id, image_url, allow_visitors)
            VALUES (
                {data.Name}, 
                {data.Description}, 
                {data.StartAt}, 
                {data.EndAt}, 
                {data.VenueId}, 
                {data.ImageUrl}, 
                {data.AllowVisitors}
                )
            "
        );

        await sql.ExecuteAsync();
    }

    public async Task Update(CreateEventRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            UPDATE events 
            SET name = {data.Name},
                description = {data.Description}, 
                start_at = {data.StartAt}, 
                end_at = {data.EndAt}, 
                venue_id = {data.VenueId},
                image_url = {data.ImageUrl},
                allow_visitors = {data.AllowVisitors}
            WHERE event_id = {data.EventId}
            "
        );

        await sql.ExecuteAsync();
    }
}
