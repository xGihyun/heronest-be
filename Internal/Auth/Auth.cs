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
    private readonly NpgsqlDataSource dataSource;
    private readonly IUserRepository userRepository;

    public AuthRepository(NpgsqlDataSource dataSource, IUserRepository userRepository)
    {
        this.dataSource = dataSource;
        this.userRepository = userRepository;
    }

    public async Task Register(RegisterRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();
        await using var transaction = await conn.BeginTransactionAsync();

        var sql =
            @"
            INSERT INTO users (email, password, role)
            VALUES (@Email, @Password, @Role::role)
            RETURNING user_id
            ";

        var userId = await conn.QuerySingleAsync<Guid>(
            sql,
            new
            {
                Email = data.Email,
                Password = data.Password,
                Role = data.Role.ToString().ToLower(),
            },
            transaction: transaction
        );

        data.UserId = userId;

        await this.userRepository.CreateDetails(data, conn, transaction);
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

        await using var conn = await this.dataSource.OpenConnectionAsync();
        var user = await conn.QuerySingleAsync<GetUserResponse>(sql, data);

        return user;
    }
}
