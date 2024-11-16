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
        var data = await context.Request.ReadFromJsonAsync<VenueRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                
            };
        }

        await this.repository.Create(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
        };
    }
}
