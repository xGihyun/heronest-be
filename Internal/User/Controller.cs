using Heronest.Internal.Api;

namespace Heronest.Internal.User;

public class UserController
{
    private readonly IUserRepository repository;

    public UserController(IUserRepository userRepository)
    {
        this.repository = userRepository;
    }

    public async Task<ApiResponse> GetById(HttpContext context)
    {
        Guid userId;

        if (!Guid.TryParse(context.GetRouteValue("userId")?.ToString(), out userId))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid user ID.",
            };
        }

        UserResponse user = await this.repository.GetById(userId);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Data = user,
        };
    }

    public async Task<ApiResponse> CreateDetails(HttpContext context)
    {
        Guid userId;

        if (!Guid.TryParse(context.GetRouteValue("userId")?.ToString(), out userId))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid user ID.",
            };
        }

        var data = await context.Request.ReadFromJsonAsync<UserDetailRequest>();

        if (data is null || data.UserId != userId)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }

        await this.repository.CreateDetails(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
        };
    }
}


