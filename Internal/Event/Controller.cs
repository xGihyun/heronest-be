using Heronest.Internal.Api;

namespace Heronest.Internal.Event;

public class EventController
{
    private readonly IEventRepository repository;

    public EventController(IEventRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> Get(HttpContext context)
    {
        string pageQuery = "1";
        string limitQuery = "10";

        if (context.Request.Query.TryGetValue("page", out var pageValue))
        {
            pageQuery = pageValue.ToString();
        }

        if (context.Request.Query.TryGetValue("limit", out var limitValue))
        {
            limitQuery = limitValue.ToString();
        }

        int page;

        if (!Int32.TryParse(pageQuery, out page))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid page query.",
            };
        }

        int limit;

        if (!Int32.TryParse(limitQuery, out limit))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid limit query.",
            };
        }

        var events = await this.repository.Get(page, limit);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched events.",
            Data = events
        };
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateEventRequest>();

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
            Message = "Successfully created event.",
        };
    }
}

