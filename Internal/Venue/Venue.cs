using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Npgsql;
using Heronest.Internal.Database;

namespace Heronest.Internal.Venue;

public class CreateVenueRequest
{
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("capacity")]
    public int Capacity { get; set; }

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("image_url")]
    public string? ImageUrl { get; set; }
}

[SqlMapper(CaseType.SnakeCase)]
public class GetVenueResponse : CreateVenueRequest
{
    [Column("venue_id")]
    public Guid VenueId { get; set; }
}

public interface IVenueRepository
{
    Task Create(CreateVenueRequest data);
    Task<GetVenueResponse[]> Get(int page, int limit);
}

public class VenueRepository : IVenueRepository
{
    private NpgsqlConnection conn;

    public VenueRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<GetVenueResponse[]> Get(int page, int limit)
    {
        var sql =
            @"
            SELECT venue_id, name, description, capacity, location, image_url
            FROM venues
            OFFSET @Offset
            LIMIT @Limit
            ";

        var venues = await this.conn.QueryAsync<GetVenueResponse>(
            sql,
            new { Offset = (page - 1) * limit, Limit = limit }
        );

        return venues.ToArray();
    }

    public async Task Create(CreateVenueRequest data)
    {
        var sql =
            @"
            INSERT INTO venues (name, description, capacity, location, image_url)
            VALUES (@Name, @Description, @Capacity, @Location, @ImageUrl)
            ";

        await this.conn.ExecuteAsync(sql, data);
    }
}
