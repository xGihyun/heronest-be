using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace Heronest.Database
{
    public enum Role
    {
        [PgName("admin")]
        Admin,

        [PgName("staff")]
        Staff,

        [PgName("student")]
        Student,

        [PgName("Visitor")]
        Visitor,
    }

    [Table("users")]
    public class User
    {
        [Column("user_id")]
        [Key]
        public Guid UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = string.Empty;
    }
}
