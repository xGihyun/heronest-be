using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace Heronest.Database
{
    public enum Sex {
        [PgName("male")]
        Male,
        [PgName("female")]
        Female
    }

    [Table("user_details")]
    public class UserDetail
    {
        [Column("user_id")]
        [Key]
        public Guid UserId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("middle_name")]
        public string? MiddleName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("birth_date")]
        public DateTime BirthDate { get; set; }

        [Column("sex")]
        public string Sex { get; set; }
    }


}
