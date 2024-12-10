using Dapper;
using DapperQueryBuilder;
using Npgsql;

namespace Heronest.Features.User;

public interface IUserRepository
{
    Task<GetUserResponse[]> GetMany(GetUserFilter filter);
    Task<GetUserResponse?> GetById(Guid userId);
    Task Create(CreateUserRequest data);
    Task Update(Person data);
}

public class UserRepository : IUserRepository
{
    private readonly NpgsqlDataSource dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async Task<GetUserResponse[]> GetMany(GetUserFilter filter)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT 
                user_id, email, role, first_name, middle_name, last_name, birth_date, sex
            FROM users
            WHERE 1=1
            "
        );

        if (filter.Name is not null)
        {
            sql += $"AND users.email ILIKE {$"%{filter.Name}%"}";
            sql += $"OR users.first_name ILIKE {$"%{filter.Name}%"}";
            sql += $"OR users.middle_name ILIKE {$"%{filter.Name}%"}";
            sql += $"OR users.last_name ILIKE {$"%{filter.Name}%"}";
        }

        if (filter.Offset.HasValue && filter.Limit.HasValue)
        {
            sql += $"OFFSET {filter.Offset.Value} LIMIT {filter.Limit.Value}";
        }

        var user = await sql.QueryAsync<GetUserResponse>();

        return user.ToArray();
    }

    public async Task<GetUserResponse?> GetById(Guid userId)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            SELECT 
                user_id, email, role, first_name, middle_name, last_name, birth_date, sex
            FROM users
            WHERE user_id = ({userId})
            "
        );

        var user = await sql.QuerySingleAsync<GetUserResponse>();

        return user;
    }

    public async Task Create(CreateUserRequest user)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            INSERT INTO users 
                (email, password, role, first_name, middle_name, last_name, birth_date, sex)
            VALUES 
                (
                {user.Email}, 
                {user.Password}, 
                {user.Role.ToString().ToLower()}::role, 
                {user.FirstName}, 
                {user.MiddleName}, 
                {user.LastName},
                {user.BirthDate},
                {user.Sex.ToString().ToLower()}::sex
                )
            "
        );

        await sql.ExecuteAsync();
    }

    public async Task Update(Person user)
    {
        await using var conn = await this.dataSource.OpenConnectionAsync();

        var sql = conn.QueryBuilder(
            $@"
            UPDATE users 
            SET email = {user.Email}, 
                password = {user.Password}, 
                role = {user.Role}::role,
                first_name = {user.FirstName},
                middle_name = {user.MiddleName},
                last_name = {user.LastName},
                birth_date = {user.BirthDate},
                sex = {user.Sex}
            WHERE user_id = {user.UserId}
            "
        );

        await sql.ExecuteAsync();
    }
}
