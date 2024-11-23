using Heronest.Internal.Api;

namespace Heronest.Internal.Auth;

public class AuthController
{
    private readonly IAuthRepository repository;

    public AuthController(IAuthRepository userRepository)
    {
        this.repository = userRepository;
    }

    public async Task<ApiResponse> Register(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<RegisterRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        await this.repository.Register(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully registered.",
        };
    }

    public async Task<ApiResponse> Login(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<LoginRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        await this.repository.Login(data);

        /*var cookieOptions = new CookieOptions*/
        /*{*/
        /*    Path = "/",*/
        /*    Secure = true,*/
        /*    SameSite = SameSiteMode.None,*/
        /*};*/
        /**/
        /*context.Response.Cookies.Append("session", "I am a session!", cookieOptions);*/

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully logged in.",
        };
    }
}
