using Heronest.Internal.Api;
using Heronest.Internal.Ticket;

namespace Heronest.Internal.User;

public class UserController
{
    private readonly IUserRepository repository;
    private readonly ITicketRepository ticketRepository;

    public UserController(IUserRepository userRepository, ITicketRepository ticketRepository)
    {
        this.repository = userRepository;
        this.ticketRepository = ticketRepository;
    }

    public async Task<ApiResponse> Get(HttpContext context)
    {
        string? name = null;

        if (context.Request.Query.TryGetValue("name", out var nameValue))
        {
            name = nameValue.ToString();
        }

        var pagination = new Pagination(context);
        var paginationResult = pagination.Parse();

        var users = await this.repository.Get(
            new GetUserFilter
            {
                Limit = paginationResult.Limit,
                Page = paginationResult.Page,
                Name = name,
            }
        );

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

        try
        {
            GetUserResponse user = await this.repository.GetById(userId);

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
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "User not found.",
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

        await this.repository.Create(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully created user.",
        };
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

        var data = await context.Request.ReadFromJsonAsync<UpdateUserRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        await this.repository.Update(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully updated user.",
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

    public async Task<ApiResponse> GetTickets(HttpContext context)
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

        var tickets = await this.ticketRepository.GetByUserId(userId);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched tickets.",
            Data = tickets,
        };
    }
}
