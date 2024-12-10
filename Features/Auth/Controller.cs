using Heronest.Features.Api;

namespace Heronest.Features.Auth;

public class AuthController
{
    private readonly IAuthRepository repository;

    public AuthController(IAuthRepository userRepository)
    {
        this.repository = userRepository;
    }

    public async Task<ApiResponse> Register(HttpContext context)
    {
        CreateUserRequest? data = null;

        try
        {
            data = await context.Request.ReadFromJsonAsync<CreateUserRequest>();
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid register JSON request.",
                Error = ex,
            };
        }

        if (data is null)
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Register JSON request cannot be empty.",
            };
        }

        if (data.Role == Role.Student && !data.Email.EndsWith("@umak.edu.ph"))
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Only UMak students are accepted.",
            };
        }

        try
        {
            await this.repository.Register(data);

            return new ApiResponse
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully registered.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Server error during registration.",
                Error = ex,
            };
        }
    }

    public async Task<ApiResponse> Login(HttpContext context)
    {
        LoginRequest? data = null;

        try
        {
            data = await context.Request.ReadFromJsonAsync<LoginRequest>();
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid login JSON request.",
                Error = ex,
            };
        }

        if (data is null)
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Register JSON request cannot be empty.",
            };
        }

        try
        {
            GetUserResponse? user = await this.repository.Login(data);

            if (user is null)
            {
                return new ApiResponse
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User with the given credentials not found.",
                };
            }

            return new ApiResponse
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully logged in.",
                Data = user,
            };
        }
        catch
        {
            return new ApiResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Server error during login.",
            };
        }
    }
}
