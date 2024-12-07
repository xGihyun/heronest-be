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
        CreateUserRequest? data = null;

        try
        {
            data = await context.Request.ReadFromJsonAsync<CreateUserRequest>();
        }
        catch
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON format.",
            };
        }

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "JSON body cannot be empty.",
            };
        }

        if (data.Role == Role.Student && !data.Email.EndsWith("@umak.edu.ph"))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Only UMak students are accepted.",
            };
        }

        try
        {
            await this.repository.Register(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully registered.",
            };
        }
        catch(Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Server error during registration.",
                Error = ex
            };
        }
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

        try
        {
            Person? user = await this.repository.Login(data);

            if (user is null)
            {
                return new ApiResponse
                {
                    Status = ApiResponseStatus.Fail,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User with the given credentials not found.",
                };
            }

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully logged in.",
                Data = user,
            };
        }
        catch
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to login.",
            };
        }
    }
}
