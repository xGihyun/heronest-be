using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Database;
using Npgsql;

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
    Task<GetVenueResponse[]> Get(PaginationResult pagination);
}

public class VenueRepository : IVenueRepository
{
    private NpgsqlConnection conn;

    public VenueRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<GetVenueResponse[]> Get(PaginationResult pagination)
    {
        var sql =
            @"
            SELECT venue_id, name, description, capacity, location, image_url
            FROM venues
            ";

        var parameters = new DynamicParameters();

        if (pagination.Page.HasValue && pagination.Limit.HasValue)
        {
            sql += " OFFSET @Offset LIMIT @Limit";
            parameters.Add("Offset", (pagination.Page.Value - 1) * pagination.Limit.Value);
            parameters.Add("Limit", pagination.Limit.Value);
        }

        var venues = await this.conn.QueryAsync<GetVenueResponse>(sql, parameters);

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
