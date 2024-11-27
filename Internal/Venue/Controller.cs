using Heronest.Internal.Api;

namespace Heronest.Internal.Venue;

public class VenueController
{
    private readonly IVenueRepository repository;

    public VenueController(IVenueRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateVenueRequest>();

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
            Message = "Successfully created venue.",
        };
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

        var data = await context.Request.ReadFromJsonAsync<UpdateVenueRequest>();

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

        await this.repository.Update(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully updated venue.",
        };
    }

    public async Task<ApiResponse> Get(HttpContext context)
    {
        var pagination = new Pagination(context);
        var paginationResult = pagination.Parse();

        var venues = await this.repository.Get(paginationResult);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched venues.",
            Data = venues
        };
    }
}
