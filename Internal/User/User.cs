using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Dapper;
using Npgsql;

namespace Heronest.Internal.User;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    [EnumMember(Value = "admin")] // Doesn't work?
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
    public string Role { get; set; }
}

public class UserResponse
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; }
}

public interface IUserRepository
{
    Task<UserResponse> GetById(Guid userId);
}

public class UserRepository : IUserRepository
{
    NpgsqlConnection conn;

    public UserRepository(NpgsqlConnection conn)
    {
        this.conn = conn;
    }

    public async Task<UserResponse> GetById(Guid userId)
    {
        var sql =
            @"
            SELECT user_id, email, role 
            FROM users
            WHERE user_id = (@UserId)
            ";
        var user = await conn.QuerySingleAsync<UserResponse>(sql, new { UserId = userId });

        return user;
    }
}
