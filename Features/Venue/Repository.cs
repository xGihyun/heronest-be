using Dapper;
using DapperQueryBuilder;
using Npgsql;

namespace Heronest.Features.Venue;

public interface IVenueRepository
{
    Task<Venue[]> GetMany(GetVenueFilter filter);
    Task Create(Venue data);
    Task Update(Venue data);
}

public class VenueRepository : IVenueRepository
{
    private NpgsqlDataSource dataSource;

    public VenueRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<Venue[]> GetMany(GetVenueFilter filter)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT venue_id, name, description, location, image_url
            FROM venues
            WHERE 1=1
            "
        );

        var parameters = new DynamicParameters();

        if (filter.Name is not null)
        {
            // TODO: If this doesn't work, try the commented one.
            sql += $"AND name ILIKE {filter.Name}";
            /*sql += $"AND name ILIKE {$"%{filter.Name}%"}";*/
        }

        if (filter.Offset.HasValue && filter.Limit.HasValue)
        {
            sql += $"OFFSET {filter.Offset.Value} LIMIT {filter.Limit.Value}";
        }

        var venues = await sql.QueryAsync<Venue>();

        return venues.ToArray();
    }

    public async Task Create(Venue data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            INSERT INTO venues (name, description, location, image_url)
            VALUES ({data.Name}, {data.Description}, {data.Location}, {data.ImageUrl})
            "
        );

        await sql.ExecuteAsync();
    }

    public async Task Update(Venue data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            UPDATE venues 
            SET name = {data.Name}, 
                description = {data.Description}, 
                location = {data.Location}, 
                image_url = {data.ImageUrl}
            WHERE venue_id = {data.VenueId}
            "
        );

        await sql.ExecuteAsync();
    }
}