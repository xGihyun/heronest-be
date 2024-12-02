using Heronest.Internal.Api;

namespace Heronest.Internal.Seat;

public class SeatController
{
    private readonly ISeatRepository repository;

    public SeatController(ISeatRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> Get(HttpContext context)
    {
        Guid venueId;

        if (!Guid.TryParse(context.GetRouteValue("venueId")?.ToString(), out venueId))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid venue ID.",
            };
        }

        Guid? eventId = null;

        if (context.Request.Query.TryGetValue("eventId", out var eventIdValue))
        {
            if (!Guid.TryParse(eventIdValue.ToString(), out var parsedEventId))
            {
                throw new ArgumentException("Invalid event ID.");
            }

            eventId = parsedEventId;
        }

        if (!eventId.HasValue)
        {
            throw new ArgumentException("Invalid event ID.");
        }

        var seats = await this.repository.Get(venueId, eventId.Value);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched seats.",
            Data = seats,
        };
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateSeatRequest>();

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
            Message = "Successfully created seats.",
        };
    }

    public async Task<ApiResponse> CreateMany(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateSeatRequest[]>();

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
            await this.repository.CreateMany(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully created seats.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = ex.Message,
            };
        }
    }
}

public class SeatSectionController
{
    private readonly ISeatSectionRepository repository;

    public SeatSectionController(ISeatSectionRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateSeatSectionRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully created.",
        };
    }
}
