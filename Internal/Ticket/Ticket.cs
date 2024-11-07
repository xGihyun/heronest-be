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

public class CreateTicketRequest
{
    [Column("metadata")]
    public object? Metadata { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("event_occurrence_id")]
    public Guid EventOccurrenceId { get; set; }

    [Column("seat_id")]
    public Guid SeatId { get; set; }
}

public interface ITicketRepository
{
    Task Create(CreateTicketRequest data);
}

public class TicketRepository : ITicketRepository
{
    private NpgsqlConnection conn;

    public TicketRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Create(CreateTicketRequest data)
    {
        var sql =
            @"
        
        ";
        await conn.ExecuteAsync(sql);
    }
}

