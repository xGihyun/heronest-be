using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Internal.Api;
using Heronest.Internal.Database;
using Npgsql;

namespace Heronest.Internal.User;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Admin,
    Staff,
    Student,
    Visitor,
}

[Table("users")]
public class User
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("role")]
    public Role Role { get; set; }
}

[SqlMapper(CaseType.SnakeCase)]
public class GetUserResponse : UserDetailRequest
{
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("role")]
    public Role Role { get; set; }
}

public class CreateUserRequest : GetUserResponse
{
    [Column("password")]
    public string Password { get; set; } = "password";
}

public class UpdateUserRequest : CreateUserRequest;

public interface IUserRepository
{
    Task<GetUserResponse[]> Get(PaginationResult pagination);
    Task<GetUserResponse> GetById(Guid userId);
    Task Create(CreateUserRequest data);
    Task Update(UpdateUserRequest data);
    Task CreateDetails(UserDetailRequest data);
    Task UpdateDetails(UserDetailRequest data);
}

public class UserRepository : IUserRepository
{
    private NpgsqlConnection conn;

    public UserRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<GetUserResponse[]> Get(PaginationResult pagination)
    {
        var sql =
            @"
            SELECT 
                users.user_id, 
                users.email, 
                users.role, 
                user_details.first_name, 
                user_details.middle_name, 
                user_details.last_name,
                user_details.birth_date,
                user_details.sex
            FROM users
            JOIN user_details ON user_details.user_id = users.user_id
            ";

        var parameters = new DynamicParameters();

        if (pagination.Page.HasValue && pagination.Limit.HasValue)
        {
            sql += " OFFSET @Offset LIMIT @Limit";
            parameters.Add("Offset", (pagination.Page.Value - 1) * pagination.Limit.Value);
            parameters.Add("Limit", pagination.Limit.Value);
        }

        var user = await this.conn.QueryAsync<GetUserResponse>(sql, parameters);

        return user.ToArray();
    }

    public async Task<GetUserResponse> GetById(Guid userId)
    {
        var sql =
            @"
            SELECT 
                users.user_id, 
                users.email, 
                users.role, 
                user_details.first_name, 
                user_details.middle_name, 
                user_details.last_name,
                user_details.birth_date,
                user_details.sex
            FROM users
            JOIN user_details ON user_details.user_id = users.user_id
            WHERE users.user_id = (@UserId)
            ";
        var user = await conn.QuerySingleAsync<GetUserResponse>(sql, new { UserId = userId });

        return user;
    }

    public async Task CreateDetails(UserDetailRequest data)
    {
        var sql =
            @"
            INSERT INTO user_details (first_name, middle_name, last_name, birth_date, sex, user_id)
            VALUES (@FirstName, @MiddleName, @LastName, @BirthDate, @Sex::sex, @UserId)
            ";

        await this.conn.ExecuteAsync(
            sql,
            new
            {
                FirstName = data.FirstName,
                MiddleName = data.MiddleName,
                LastName = data.LastName,
                BirthDate = data.BirthDate,
                Sex = data.Sex.ToString().ToLower(),
                UserId = data.UserId,
            }
        );
    }

    public async Task UpdateDetails(UserDetailRequest data)
    {
        var sql =
            @"
            UPDATE user_details 
            SET first_name = @FirstName, 
                middle_name = @MiddleName, 
                last_name = @LastName, 
                birth_date = @BirthDate, 
                sex = @Sex::sex
            WHERE user_id = @UserId
            ";

        await this.conn.ExecuteAsync(
            sql,
            new
            {
                FirstName = data.FirstName,
                MiddleName = data.MiddleName,
                LastName = data.LastName,
                BirthDate = data.BirthDate,
                Sex = data.Sex.ToString().ToLower(),
                UserId = data.UserId,
            }
        );
    }

    public async Task Create(CreateUserRequest data)
    {
        var sql =
            @"
            INSERT INTO users (email, password, role)
            VALUES (@Email, @Password, @Role::role)
            RETURNING user_id
            ";

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

        await this.CreateDetails(data);
    }

    public async Task Update(UpdateUserRequest data)
    {
        var sql =
            @"
            UPDATE users 
            SET email = @Email, 
                password = @Password, 
                role = @Role::role
            WHERE user_id = @UserId
            ";

        await this.conn.QueryAsync<Guid>(
            sql,
            new
            {
                Email = data.Email,
                Password = data.Password,
                Role = data.Role.ToString().ToLower(),
                UserId = data.UserId
            }
        );

        await this.UpdateDetails(data);
    }
}
