using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Heronest.Features.Database;
using Heronest.Features.Event;
using Heronest.Features.Venue;

namespace Heronest.Features.Seat;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeatStatus
{
    Reserved,
    Available,
    Unavailable,
}

[SqlMapper(CaseType.SnakeCase)]
public record Seat
{
    [Column("seat_id")]
    public Guid SeatId { get; set; }

    [Column("seat_number")]
    public string SeatNumber { get; set; } = String.Empty;

    [Column("seat_section_id")]
    public Guid? SeatSectionId { get; set; }

    [Column("venue_id")]
    public Guid VenueId { get; set; }

    [Column("metadata")]
    public dynamic Metadata { get; set; } = new object { };

    public SeatReservation? Reservation { get; set; }

    [JsonIgnore]
    public string? ReservationJson { get; set; }
}

public record SeatReservation(
    DateTime ReservedAt,
    string TicketNumber,
    UserBriefDetail User,
    /*VenueBriefDetail Venue,*/
    EventBriefDetail Event
);

public record CreateSeatRequest(
    [property: Column("seat_id")] Guid SeatId,
    [property: Column("seat_number")] string SeatNumber,
    [property: Column("seat_section_id")] Guid? SeatSectionId,
    [property: Column("venue_id")] Guid VenueId,
    [property: Column("metadata")] dynamic Metadata,
    SeatBriefReservationDetail? Reservation
);

public record SeatBriefReservationDetail(Guid UserId, Guid EventId);

public record SeatBriefDetail(Guid SeatId, string SeatNumber);


public record GetSeatFilter(Guid? EventId);
