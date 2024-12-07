using System.ComponentModel.DataAnnotations.Schema;

namespace Heronest.Internal.Auth;

public record LoginRequest( 
    [property: Column("email")] string Email,
    [property: Column("password")] string Password 
);
