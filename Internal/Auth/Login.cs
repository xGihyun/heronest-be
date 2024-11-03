namespace Heronest.Internal.Auth;

using System.ComponentModel.DataAnnotations.Schema;

public class LoginRequest
{
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;
}
