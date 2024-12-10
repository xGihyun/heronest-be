using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Features.Database;

namespace Heronest.Features.Event;

using Venue = Heronest.Features.Venue.Venue;

[SqlMapper(CaseType.SnakeCase)]
public record Event
{
    [Column("name")]
    public string Name { get; set; } = String.Empty;

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

    [Column("allow_visitors")]
    public bool AllowVisitors { get; set; }

    [Column("total_reservation")]
    public int TotalReservation { get; set; }

    public Venue Venue { get; set; } = new Venue();

    [JsonIgnore]
    public string VenueJson { get; set; } = string.Empty;
}

public record CreateEventRequest(
    [property: Column("event_id")] Guid EventId,
    [property: Column("name")] String Name,
    [property: Column("description")] String? Description,
    [property: Column("start_at")] DateTime StartAt,
    [property: Column("end_at")] DateTime EndAt,
    [property: Column("venue_id")] Guid VenueId,
    [property: Column("image_url")] string? ImageUrl,
    [property: Column("allow_visitors")] bool AllowVisitors
);

public record EventBriefDetail(Guid EventId, string Name, DateTime StartAt, DateTime EndAt);

public record GetEventFilter(int? Offset, int? Limit, string? Name, Guid? VenueId);
