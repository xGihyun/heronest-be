using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;

namespace Heronest.Internal.Seat;

public class CreateSeatSectionRequest
{
    [Column("name")]
    public String Name { get; set; } = String.Empty;

    [Column("description")]
    public String? Description { get; set; }
}

public interface ISeatSectionRepository
{
    Task Create(CreateSeatSectionRequest data);
}

public class SeatSectionRepository : ISeatSectionRepository
{
    private NpgsqlDataSource dataSource;

    public SeatSectionRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task Create(CreateSeatSectionRequest data)
    {
        var sql =
            @"
            INSERT INTO seats(name, description)
            VALUES(@Name, @Description)
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        await conn.ExecuteAsync(sql, data);
    }
}

