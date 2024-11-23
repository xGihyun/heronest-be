namespace Heronest.Internal.Auth;

using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Npgsql;

public class LoginRequest
{
    [Column("email")]
    public string Email { get; set; } = String.Empty;

    [Column("password")]
    public string? Password { get; set; } = String.Empty;
}

public interface ILoginRepository
{
    Task Create(LoginRequest data);
}

public class LoginRepository : ILoginRepository
{
    private NpgsqlConnection conn;

    public LoginRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Create(LoginRequest data)
    {
        var sql =
            @"
        SELECT * FROM users 
        WHERE email = @Email AND password = @Password;
        ";

        await conn.ExecuteAsync(sql);
    }
}
