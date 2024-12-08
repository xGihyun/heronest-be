using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dapper;
using Heronest.Features.Database;
using NpgsqlTypes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    [PgName("admin")]
    Admin,

    [PgName("student")]
    Student,

    [PgName("visitor")]
    Visitor,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Sex
{
    [PgName("male")]
    Male,

    [PgName("female")]
    Female,
}

[SqlMapper(CaseType.SnakeCase)]
public record Person(
    [property: Column("user_id")] Guid UserId,
    [property: Column("email")] string Email,
    [property: Column("password")] string Password,
    [property: Column("role")] Role Role,
    [property: Column("first_name")] string FirstName,
    [property: Column("middle_name")] string? MiddleName,
    [property: Column("last_name")] string LastName,
    [property: Column("birth_date")] DateTime BirthDate,
    [property: Column("sex")] Sex Sex
);

[SqlMapper(CaseType.SnakeCase)]
public record GetUserResponse
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("role")]
    public Role Role { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("middle_name")]
    public string? MiddleName { get; set; }

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("birth_date")]
    public DateTime BirthDate { get; set; }

    [Column("sex")]
    public Sex Sex { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
}

public record CreateUserRequest(
    [property: Column("email")] string Email,
    [property: Column("password")] string Password,
    [property: Column("role")] Role Role,
    [property: Column("first_name")] string FirstName,
    [property: Column("middle_name")] string? MiddleName,
    [property: Column("last_name")] string LastName,
    [property: Column("birth_date")] DateTime BirthDate,
    [property: Column("sex")] Sex Sex
);

public record GetUserFilter(string? Name, int? Offset, int? Limit);

public record UserBriefDetail(Guid UserId, string FirstName, string? MiddleName, string LastName);
