namespace Heronest.Internal.Auth;

public class AuthService
{
    private readonly IAuthRepository _repository;

    public AuthService(IAuthRepository userRepository)
    {
        _repository = userRepository;
    }

    public async Task HandleRegister(HttpContext httpContext)
    {
        var data = await httpContext.Request.ReadFromJsonAsync<RegisterRequest>();

        if (data is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await _repository.Register(data);

        httpContext.Response.StatusCode = StatusCodes.Status201Created;
    }

    public async Task HandleLogin(HttpContext httpContext)
    {
        var data = await httpContext.Request.ReadFromJsonAsync<LoginRequest>();

        if (data is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await _repository.Login(data);

        var cookieOptions = new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None,
        };

        httpContext.Response.Cookies.Append("session", "I am a session!", cookieOptions);
    }
}
