using System.ComponentModel.DataAnnotations.Schema;
using Heronest.Features.Database;

namespace Heronest.Features.Venue;

[SqlMapper(CaseType.SnakeCase)]
public record Venue
{
    [Column("venue_id")]
    public Guid VenueId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("image_url")]
    public string? ImageUrl { get; set; }
};

public record GetVenueFilter(int? Offset, int? Limit, string? Name);

public record VenueBriefDetail(Guid VenueId, string Name);
