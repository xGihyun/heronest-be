using System.ComponentModel.DataAnnotations.Schema;

namespace Heronest.Features.Auth;

public record LoginRequest( 
    [property: Column("email")] string Email,
    [property: Column("password")] string Password 
);
