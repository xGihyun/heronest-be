using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Heronest.Internal.User;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Sex
{
    [PgName("male")]
    Male,
    [PgName("female")]
    Female,
}

public class UserDetailRequest
{
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

    [Column("user_id")]
    public Guid UserId { get; set; }
}
