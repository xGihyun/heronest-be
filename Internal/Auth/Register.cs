namespace Heronest.Internal.Auth;

using Heronest.Internal.User;

public class RegisterRequest : UserDetailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; }
}
