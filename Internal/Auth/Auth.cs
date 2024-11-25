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
    private readonly NpgsqlConnection conn;

    public AuthRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task Register(RegisterRequest data)
    {
        NpgsqlTransaction transaction = await this.conn.BeginTransactionAsync();

        if(transaction.Connection == null) {
            throw new Exception("Transaction connection is null.");
        }

        var sql =
            @"
            INSERT INTO users (email, password, role)
            VALUES (@Email, @Password, @Role::role)
            RETURNING user_id
            ";

        var userRepo = new UserRepository(transaction.Connection);

        var userId = await this.conn.QuerySingleAsync<Guid>(
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
        await transaction.CommitAsync();
    }

    public async Task<GetUserResponse> Login(LoginRequest data)
    {
        var sql =
            @"
            SELECT user_id, email, role 
            FROM users 
            WHERE email = @Email AND password = @Password
            ";

        var user = await this.conn.QuerySingleAsync<GetUserResponse>(sql, data);

        return user;
    }
}
