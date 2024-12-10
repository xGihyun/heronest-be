using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Features.Database;
using Heronest.Features.Event;
using Heronest.Features.Seat;
using Heronest.Features.Venue;

namespace Heronest.Features.Ticket;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TicketStatus
{
    Reserved,
    Used,
    Canceled,
}

[SqlMapper(CaseType.SnakeCase)]
public record Ticket
{
    [Column("ticket_id")]
    public Guid TicketId { get; set; }

    [Column("ticket_number")]
    public string TicketNumber { get; set; } = string.Empty;

    [Column("reserved_at")]
    public DateTime ReservedAt { get; set; }

    [Column("status")]
    public TicketStatus Status { get; set; }

    public TicketReservation Reservation { get; set; } = default!;

    [JsonIgnore]
    public string ReservationJson { get; set; } = string.Empty;
}

public record TicketReservation(
    VenueBriefDetail Venue,
    EventBriefDetail Event,
    UserBriefDetail User,
    SeatBriefDetail Seat
);

public record CreateTicketRequest(
    [property: Column("user_id")] Guid UserId,
    [property: Column("seat_id")] Guid SeatId,
    [property: Column("event_id")] Guid EventId
);

public record UpdateTicketRequest(
    [property: Column("ticket_id")] Guid TicketId,
    [property: Column("status")] TicketStatus Status
);

public record GetTicketFilter(int? Offset, int? Limit, Guid? EventId, Guid? UserId);
