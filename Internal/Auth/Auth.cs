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
    private readonly NpgsqlConnection conn;

    public AuthRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Register(RegisterRequest data)
    {
        var sql =
            @"
            INSERT INTO users (email, password, role)
            VALUES (@Email, @Password, @Role::role)
            ";

        await this.conn.ExecuteAsync(   
            sql,
            new
            {
                Email = data.Email,
                Password = data.Password,
                Role = data.Role.ToString().ToLower(),
            }
        );
    }

    public async Task Login(LoginRequest data)
    {
        var sql =
            @"
            SELECT user_id, email, role 
            FROM users 
            WHERE email = @Email AND password = @Password
            ";

        await this.conn.QuerySingleAsync(sql, data);
    }
}
