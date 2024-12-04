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
        Guid? venueId = null;

        if (context.Request.Query.TryGetValue("venueId", out var venueIdValue))
        {
            if (!Guid.TryParse(venueIdValue.ToString(), out var parsedVenueId))
            {
                throw new ArgumentException("Invalid venue ID.");
            }

            venueId = parsedVenueId;
        }

        string? name = null;

        if (context.Request.Query.TryGetValue("name", out var nameValue))
        {
            name = nameValue.ToString();
        }

        var pagination = new Pagination(context);
        var paginationResult = pagination.Parse();

        var events = await this.repository.Get(
            new GetEventFilter
            {
                Limit = paginationResult.Limit,
                Page = paginationResult.Page,
                VenueId = venueId,
                Name = name
            }
        );

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

        var data = await context.Request.ReadFromJsonAsync<UpdateEventRequest>();

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

        await this.repository.Update(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully updated event.",
        };
    }
}
