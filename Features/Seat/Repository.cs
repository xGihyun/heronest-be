using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using DapperQueryBuilder;
using Heronest.Features.Ticket;
using Npgsql;

namespace Heronest.Features.Seat;

public interface ISeatRepository
{
    Task<Seat[]> GetMany(Guid venueId, GetSeatFilter filter);
    Task Create(CreateSeatRequest data);
    Task CreateMany(CreateSeatRequest[] data);
}

public class SeatRepository : ISeatRepository
{
    private readonly NpgsqlDataSource dataSource;
    private readonly ITicketRepository ticketRepository;

    public SeatRepository(NpgsqlDataSource dataSource, ITicketRepository ticketRepository)
    {
        this.dataSource = dataSource;
        this.ticketRepository = ticketRepository;
    }

    public async Task<Seat[]> GetMany(Guid venueId, GetSeatFilter filter)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT 
                seats.seat_id,
                seats.seat_number, 
                seats.seat_section_id, 
                seats.venue_id, 
                seats.metadata, 
            "
        );

        if (filter.EventId.HasValue)
        {
            sql +=
                @$"
                CASE 
                    WHEN users.user_id IS NOT NULL THEN 
                        jsonb_build_object (
                            'reserved_at', tickets.created_at,
                            'ticket_number', tickets.ticket_number,
                            'user', jsonb_build_object(
                                'user_id', users.user_id,
                                'first_name', users.first_name,
                                'middle_name', users.middle_name,
                                'last_name', users.last_name
                            ),
                            'event', jsonb_build_object(
                                'event_id', events.event_id,
                                'name', events.name,
                                'start_at', events.start_at,
                                'end_at', events.end_at
                            )
                        )
                    ELSE NULL
                END AS reservation_json
            FROM seats
                ";
        }
        else
        {
            sql +=
                @$"
                NULL as reservation_json
            FROM seats
                ";
        }

        if (filter.EventId.HasValue)
        {
            sql +=
                @$"
                LEFT JOIN tickets 
                    ON tickets.seat_id = seats.seat_id  
                AND tickets.event_id = {filter.EventId}
                LEFT JOIN users ON users.user_id = tickets.user_id
                LEFT JOIN events ON events.event_id = tickets.event_id
            ";
        }

        sql +=
            $@"
            WHERE seats.venue_id = {venueId} 
            ORDER BY seats.seat_number::int
        ";

        var seats = await sql.QueryAsync<Seat>();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower));

        seats = seats.Select(seat =>
        {
            if (seat.ReservationJson is not null)
            {
                seat.Reservation =
                    JsonSerializer.Deserialize<SeatReservation>(seat.ReservationJson, options)
                    ?? throw new JsonException("Failed to deserialize seat reservation details.");
            }

            return seat;
        });

        return seats.ToArray();
    }

    public async Task Create(CreateSeatRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        var metadataJson = JsonSerializer.Serialize(data.Metadata);

        var sql = conn.QueryBuilder(
            $@"
            INSERT INTO seats (seat_id, seat_number, seat_section_id, venue_id, metadata)
            VALUES (
                {data.SeatId}, 
                {data.SeatNumber}, 
                {data.SeatSectionId}, 
                {data.VenueId}, 
                {metadataJson}::jsonb
                )
            ON CONFLICT(seat_id)
            DO UPDATE SET
                seat_number = {data.SeatNumber},
                seat_section_id = {data.SeatSectionId},
                {metadataJson}::jsonb
            "
        );

        await sql.ExecuteAsync(transaction: transaction);

        if (data.Reservation is not null)
        {
            await this.ticketRepository.Create(
                new CreateTicketRequest(
                    data.Reservation.UserId,
                    data.SeatId,
                    data.Reservation.EventId
                )
            );
        }

        await transaction.CommitAsync();
    }

    public async Task CreateMany(CreateSeatRequest[] data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        try
        {
            foreach (var seat in data)
            {
                string metadataJson;
                if (seat.Metadata is JsonElement jsonElement)
                {
                    metadataJson = jsonElement.ToString();
                }
                else
                {
                    Console.WriteLine("Seat.Metadata is not a JsonElement.");
                    metadataJson = JsonSerializer.Serialize(seat.Metadata);
                }

                var sql = conn.QueryBuilder(
                    $@"
                    INSERT INTO seats (seat_id, seat_number, seat_section_id, venue_id, metadata)
                    VALUES (
                        {seat.SeatId}, 
                        {seat.SeatNumber}, 
                        {seat.SeatSectionId}, 
                        {seat.VenueId}, 
                        {metadataJson}::jsonb
                        )
                    ON CONFLICT(seat_id)
                    DO UPDATE SET
                        seat_number = {seat.SeatNumber},
                        seat_section_id = {seat.SeatSectionId},
                        metadata = {metadataJson}::jsonb
                    "
                );

                await sql.ExecuteAsync(transaction: transaction);

                if (seat.Reservation is not null)
                {
                    await this.ticketRepository.Create(
                        new CreateTicketRequest(
                            seat.Reservation.UserId,
                            seat.SeatId,
                            seat.Reservation.EventId
                        )
                    );
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
