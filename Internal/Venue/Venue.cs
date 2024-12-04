using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Database;
using Npgsql;

namespace Heronest.Internal.Venue;

public class CreateVenueRequest
{
    [Column("venue_id")]
    public Guid VenueId { get; set; }

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

public class UpdateVenueRequest : CreateVenueRequest;

public class GetVenueFilter : PaginationResult
{
    public string? Name { get; set; }
}

[SqlMapper(CaseType.SnakeCase)]
public class GetVenueResponse : CreateVenueRequest;

public interface IVenueRepository
{
    Task Create(CreateVenueRequest data);
    Task Update(UpdateVenueRequest data);
    Task<GetVenueResponse[]> Get(GetVenueFilter filter);
}

public class VenueRepository : IVenueRepository
{
    private NpgsqlDataSource dataSource;

    public VenueRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<GetVenueResponse[]> Get(GetVenueFilter filter)
    {
        var sql =
            @"
            SELECT venue_id, name, description, capacity, location, image_url
            FROM venues
            ";

        var parameters = new DynamicParameters();

        if (filter.Name is not null)
        {
            sql +=
                @" 
                WHERE name ILIKE @Name
                ";
            parameters.Add("Name", $"%{filter.Name}%");
        }

        if (filter.Page.HasValue && filter.Limit.HasValue)
        {
            sql += " OFFSET @Offset LIMIT @Limit";
            parameters.Add("Offset", (filter.Page.Value - 1) * filter.Limit.Value);
            parameters.Add("Limit", filter.Limit.Value);
        }

        await using var conn = await this.dataSource.OpenConnectionAsync();
        var venues = await conn.QueryAsync<GetVenueResponse>(sql, parameters);

        return venues.ToArray();
    }

    public async Task Create(CreateVenueRequest data)
    {
        var sql =
            @"
            INSERT INTO venues (name, description, capacity, location, image_url)
            VALUES (@Name, @Description, @Capacity, @Location, @ImageUrl)
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }

    public async Task Update(UpdateVenueRequest data)
    {
        var sql =
            @"
            UPDATE venues 
            SET name = @Name, 
                description = @Description, 
                capacity = @Capacity, 
                location = @Location, 
                image_url = @ImageUrl
            WHERE venue_id = @VenueId
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }
}
