using Heronest.Features.Api;

namespace Heronest.Features.User;

public class UserController
{
    private readonly IUserRepository userRepository;

    public UserController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<ApiResponse> GetMany(HttpContext context)
    {
        string? name = QueryParameter.TryGetValue<string>(context, "name");

        try
        {
            var pagination = new PaginationQuery(context);
            var (offset, limit) = pagination.GetValues();

            var filter = new GetUserFilter(name, offset, limit);
            var users = await this.userRepository.GetMany(filter);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully fetched users.",
                Data = users,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to get users.",
                Data = new Person[] { },
                Error = ex,
            };
        }
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

        try
        {
            GetUserResponse? user = await this.userRepository.GetById(userId);

            if (user is null)
            {
                return new ApiResponse
                {
                    Status = ApiResponseStatus.Fail,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found.",
                };
            }

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Data = user,
                Message = "Successfully fetched user.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to get user.",
                Error = ex,
            };
        }
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

        try
        {
            await this.userRepository.Create(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully created user.",
            };
        }
        catch
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to create user.",
            };
        }
    }

    public async Task<ApiResponse> Update(HttpContext context)
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

        var data = await context.Request.ReadFromJsonAsync<Person>();

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
            await this.userRepository.Update(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully updated user.",
            };
        }
        catch
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to update user.",
            };
        }
    }

    public async Task<ApiResponse> UpdateAvatar(HttpContext context)
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

        var data = await context.Request.ReadFromJsonAsync<UserAvatar>();

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
            await this.userRepository.UpdateAvatar(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully updated user avatar.",
            };
        }
        catch
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to update user avatar.",
            };
        }
    }
}
