using Heronest.Internal.Api;
using Heronest.Internal.User;

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

        GetUserResponse user = await this.repository.Login(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully logged in.",
            Data = user,
        };
    }
}
