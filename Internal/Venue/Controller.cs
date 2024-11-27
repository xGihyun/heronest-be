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
