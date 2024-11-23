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
    private NpgsqlConnection conn;

    public SeatSectionRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Create(CreateSeatSectionRequest data)
    {
        var sql =
            @"
            INSERT INTO seats(name, description)
            VALUES(@Name, @Description)
            ";

        await conn.ExecuteAsync(sql, data);
    }
}

