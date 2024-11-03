using Dapper;
using Npgsql;

namespace Heronest.Internal.Auth;

public interface IAuthRepository
{
    Task Register(RegisterRequest data);
    Task Login(LoginRequest data);
}

public class AuthRepository : IAuthRepository
{
    private readonly NpgsqlConnection _conn;

    public AuthRepository(NpgsqlConnection conn)
    {
        this._conn = conn;
    }

    public async Task Register(RegisterRequest data)
    {
        var sql =
            @"
            INSERT INTO users (email, password, role)
            VALUES (@Email, @Password, @Role::role)
            ";

        await this._conn.ExecuteAsync(sql, data);
    }

    public async Task Login(LoginRequest data)
    {
        var sql =
            @"
            SELECT user_id, email, role 
            FROM users 
            WHERE email = @Email AND password = @Password
            ";

        await this._conn.QuerySingleAsync(sql, data);
    }
}
