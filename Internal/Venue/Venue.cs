using System.ComponentModel.DataAnnotations.Schema;
using Npgsql;
using Dapper;

namespace Heronest.Internal.Venue;

public class VenueRequest
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

public interface IVenueRepository
{
    Task Create(VenueRequest data);
}

public class VenueRepository : IVenueRepository
{
    private NpgsqlConnection conn;

    public VenueRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Create(VenueRequest data)
    {
        var sql =
            @"
            INSERT INTO venues (name, description, capacity, location, image_url)
            VALUES (@Name, @Description, @Capacity, @Location, @ImageUrl)
            ";

        await this.conn.ExecuteAsync(sql);
    }
}


