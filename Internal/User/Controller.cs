using Heronest.Internal.Api;

namespace Heronest.Internal.User;

public class UserController
{
    private readonly IUserRepository repository;

    public UserController(IUserRepository userRepository)
    {
        this.repository = userRepository;
    }

    public async Task<ApiResponse> Get(HttpContext context)
    {
        var pagination = new Pagination(context);
        var paginationResult = pagination.Parse();

        var users = await this.repository.Get(paginationResult);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched users.",
            Data = users,
        };
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

        GetUserResponse user = await this.repository.GetById(userId);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Data = user,
        };
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateUserRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        await this.repository.Create(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully created user.",
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
            Message = "Successfully created user details.",
        };
    }
}
