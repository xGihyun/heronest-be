using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
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

public class UserResponse
{
    
    [Column("user_id")]
    public Guid UserId { get; set; } // data type (guid)

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("role")]
    public Role Role { get; set; }
}

public interface IUserRepository
{
    Task<UserResponse> GetById(Guid userId);
    Task CreateDetails(UserDetailRequest data);
}

public class UserRepository : IUserRepository
{
    private NpgsqlConnection conn;

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

   
    // RegisterUser -> INSERT INTO user_details(user_id)  VALUES (@UserId)
    // put a guid value in my UserId. 


    
}
