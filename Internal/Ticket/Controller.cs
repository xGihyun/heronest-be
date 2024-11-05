using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using Npgsql;

namespace Heronest.Internal.Ticket;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TicketStatus
{
    Reserved,
    Used,
    Canceled,
}

public class TicketRequest { 
    [Column("metadata")]
    public object? Metadata {get; set;}

    [Column("user_id")]
    public Guid UserId {get; set;}

    [Column("event_id")]
    public Guid EventId {get; set;}

    [Column("seat_id")]
    public Guid SeatId {get; set;}
}
