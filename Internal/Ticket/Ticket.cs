using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;
using NanoidDotNet;
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

public class CreateTicketResponse
{
    [Column("ticket_number")]
    public String TicketNumber { get; set; } = String.Empty;
}

public class UpdateTicketRequest
{

    [Column("ticket_id")]
    public Guid TicketId { get; set; }


    [Column("status")]
    public TicketStatus Status { get; set; }

}
public interface ITicketRepository
{

    Task<CreateTicketResponse> Create(CreateTicketRequest data);
    Task Update(UpdateTicketRequest data);
}

public class TicketRepository : ITicketRepository
{
    private NpgsqlConnection conn;

    public TicketRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<CreateTicketResponse> Create(CreateTicketRequest data)
    {
        var TicketNumberId = Nanoid.Generate(size: 10);
        var sql = @"
        INSERT INTO tickets(metadata, user_id, event_occurrence_id, seat_id, ticket_number)
        VALUES(@Metadata, @UserId, @EventOccurrenceId, @SeatId, @TicketNumber)
        RETURNING ticket_id
        ";
        await conn.ExecuteAsync(sql, new
        {
            Metadata = data.Metadata,
            UserId = data.UserId,
            EventOccurrenceId = data.EventOccurrenceId,
            SeatId = data.SeatId,
            TicketNumber = TicketNumberId,  // Pass the generated TicketNumber here
          
        });
        return new CreateTicketResponse { TicketNumber = TicketNumberId };
    }


    // Update   
   public async Task Update(UpdateTicketRequest data)
{
    var sql =
    @"
    UPDATE tickets 
    SET status = @Status
    WHERE ticket_id = @TicketId
    ";

    await conn.ExecuteAsync(sql, new { Status = data.Status, TicketId = data.TicketId });
}


}

