using Heronest.Internal.Api;

namespace Heronest.Internal.Seat;

public class SeatController
{
    private readonly ISeatRepository repository;
    
    public SeatController(ISeatRepository seatRepository )
    {
        this.repository = seatRepository;
        
    }

    public async Task<ApiResponse> CreateSeat(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateSeatRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
        await this.repository.Create(data);
        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Created Successfully"
        };
    }
}

public class SeatSectionController
{
    private readonly ISeatSectionRepository repository; 

    public SeatSectionController(ISeatSectionRepository IseatSectionrepository)
    {
        this.repository = IseatSectionrepository;
    }

    public async Task<ApiResponse> CreateSeatSection(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateSeatSectionRequest>();

        if(data is null ) 
        {
            return new ApiResponse 
            {
                Status = ApiResponseStatus.Fail, 
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success, 
            StatusCode = StatusCodes.Status201Created, 
            Message = "Connection Successful"
        };

    }


}