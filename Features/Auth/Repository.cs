using Dapper;
using DapperQueryBuilder;
using Heronest.Features.User;
using Npgsql;

namespace Heronest.Features.Auth;

public interface IAuthRepository
{
    Task Register(CreateUserRequest data);
    Task<Person?> Login(LoginRequest data);
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

    public async Task Register(CreateUserRequest data)
    {
        await this.userRepository.Create(data);
    }

    public async Task<Person?> Login(LoginRequest data)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT 
                user_id, email, role, first_name, middle_name, last_name, birth_date, sex
            FROM users
            WHERE email = ({data.Email}) AND password = ({data.Password})
            "
        );

        var user = await sql.QuerySingleAsync<Person>();

        return user;
    }
}
