using Heronest.Features.Api;

namespace Heronest.Features.Event;

public class EventController
{
    private readonly IEventRepository repository;

    public EventController(IEventRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> GetMany(HttpContext context)
    {
        Guid? venueId = QueryParameter.TryGetValueFromStruct<Guid>(context, "venueId");
        string? name = QueryParameter.TryGetValue<string>(context, "name");

        try
        {
            var pagination = new PaginationQuery(context);
            var (offset, limit) = pagination.GetValues();

            var events = await this.repository.GetMany(
                new GetEventFilter(offset, limit, name, venueId)
            );

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully fetched events.",
                Data = events,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to fetch events.",
                Data = new Event[] { },
                Error = ex,
            };
        }
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

        try
        {
            await this.repository.Create(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully created event.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to create event.",
                Error = ex,
            };
        }
    }

    public async Task<ApiResponse> Update(HttpContext context)
    {
        Guid eventId;

        if (!Guid.TryParse(context.GetRouteValue("eventId")?.ToString(), out eventId))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid event ID.",
            };
        }

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

        if (data.EventId != eventId)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Event IDs don't match.",
            };
        }

        try
        {
            await this.repository.Update(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully updated event.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to update event.",
                Error = ex,
            };
        }
    }
}
