using Heronest.Features.Api;

namespace Heronest.Features.Venue;

public class VenueController
{
    private readonly IVenueRepository repository;

    public VenueController(IVenueRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> GetMany(HttpContext context)
    {
        string? name = QueryParameter.TryGetValue<string>(context, "name");

        try
        {
            var pagination = new PaginationQuery(context);
            var (offset, limit) = pagination.GetValues();

            var filter = new GetVenueFilter(offset, limit, name);
            var venues = await this.repository.GetMany(filter);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully fetched venues.",
                Data = venues,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to fetch venues.",
                Error = ex,
            };
        }
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<Venue>();

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
                Message = "Successfully created venue.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to create venue.",
                Error = ex,
            };
        }
    }

    public async Task<ApiResponse> Update(HttpContext context)
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

        var data = await context.Request.ReadFromJsonAsync<Venue>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        if (data.VenueId != venueId)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Venue IDs don't match.",
            };
        }

        try
        {
            await this.repository.Update(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully updated venue.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to update venue.",
                Error = ex,
            };
        }
    }
}
