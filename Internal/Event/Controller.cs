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
        var pagination = new Pagination(context);
        var paginationResult = pagination.Parse();

        var events = await this.repository.Get(paginationResult);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched events.",
            Data = events,
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
