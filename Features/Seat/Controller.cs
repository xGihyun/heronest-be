using Heronest.Features.Api;
using Npgsql;

namespace Heronest.Features.Seat;

public class SeatController
{
    private readonly ISeatRepository repository;

    public SeatController(ISeatRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> GetMany(HttpContext context)
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

        Guid? eventId = QueryParameter.TryGetValueFromStruct<Guid>(context, "eventId");

        try
        {
            var filter = new GetSeatFilter(eventId);
            var seats = await this.repository.GetMany(venueId, filter);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully fetched seats.",
                Data = seats,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to fetch seats.",
                Data = new Seat[] { },
                Error = ex,
            };
        }
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

        try
        {
            await this.repository.Create(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Success.",
            };
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status409Conflict,
                Message = "User has an existing reservation.",
            };
        }
        catch
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to reserve seat.",
            };
        }
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
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status409Conflict,
                Message = "User has an existing reservation.",
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

// NOTE: Not implemented (yet)
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
