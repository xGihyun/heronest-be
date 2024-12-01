using Dapper;
using Heronest.Internal.User;
using Npgsql;

namespace Heronest.Internal.Auth;

public interface IAuthRepository
{
    Task Register(RegisterRequest data);
    Task<GetUserResponse> Login(LoginRequest data);
}

public class AuthRepository : IAuthRepository
{
    private NpgsqlDataSource dataSource;

    public AuthRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task Register(RegisterRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();
        /*await using var txn = await conn.BeginTransactionAsync();*/

        var sql =
            @"
            INSERT INTO users (email, password, role)
            VALUES (@Email, @Password, @Role::role)
            RETURNING user_id
            ";

        // TODO: Pass the connection instead
        var userRepo = new UserRepository(this.dataSource);

        var userId = await conn.QuerySingleAsync<Guid>(
            sql,
            new
            {
                Email = data.Email,
                Password = data.Password,
                Role = data.Role.ToString().ToLower(),
            }
        );

        data.UserId = userId;

        await userRepo.CreateDetails(data);
        /*await txn.CommitAsync();*/
    }

    public async Task<GetUserResponse> Login(LoginRequest data)
    {
        var sql =
            @"
            SELECT user_id, email, role 
            FROM users 
            WHERE email = @Email AND password = @Password
            ";

        await using var conn = await this.dataSource.OpenConnectionAsync();
        var user = await conn.QuerySingleAsync<GetUserResponse>(sql, data);

        return user;
    }
}
