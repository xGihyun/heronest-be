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

        if (
            !context.Request.Query.TryGetValue("venueId", out var data)
            || !Guid.TryParse(data.ToString(), out venueId)
        )
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid or missing venue ID.",
            };
        }

        var seats = await this.repository.Get(venueId);

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

        await this.repository.Create(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully created.",
        };
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
