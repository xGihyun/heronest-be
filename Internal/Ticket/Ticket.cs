using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using NanoidDotNet;
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
    public dynamic? Metadata { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("seat_id")]
    public Guid SeatId { get; set; }

    [Column("event_id")]
    public Guid EventId { get; set; }
}

public class CreateTicketResponse
{
    [Column("ticket_number")]
    public string TicketNumber { get; set; } = string.Empty;
}

public class GetTicketResponse
{
    [Column("ticket_id")]
    public Guid TicketId { get; set; }

    [Column("ticket_number")]
    public string TicketNumber { get; set; } = string.Empty;

    [Column("status")]
    public TicketStatus Status { get; set; }

    [Column("metadata")]
    public dynamic? Metadata { get; set; }

    // TODO:
    // User data
    // Seat data
    // Event data
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
    private NpgsqlDataSource dataSource;

    public TicketRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<CreateTicketResponse> Create(CreateTicketRequest data)
    {
        var ticketNumber = Nanoid.Generate(size: 10);
        var sql =
            @"
            INSERT INTO tickets (metadata, user_id, seat_id, event_id, ticket_number, status)
            VALUES (@Metadata, @UserId, @SeatId, @EventId, @TicketNumber, 'reserved')
            RETURNING ticket_id
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(
            sql,
            new
            {
                Metadata = data.Metadata,
                UserId = data.UserId,
                SeatId = data.SeatId,
                EventId = data.EventId,
                TicketNumber = ticketNumber,
            }
        );

        return new CreateTicketResponse { TicketNumber = ticketNumber };
    }

    public async Task Update(UpdateTicketRequest data)
    {
        var sql =
            @"
            UPDATE tickets 
            SET status = @Status
            WHERE ticket_id = @TicketId
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }
}
