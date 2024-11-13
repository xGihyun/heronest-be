using Heronest.Internal.Api;


namespace Heronest.Internal.Event; 

public class EventController
{
    private readonly IEventRepository repository; 
    
    public EventController(IEventRepository eventRepository)
    {
        this.repository = eventRepository;
    }

    public async Task<ApiResponse> CreateEvent(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateEventRequest>();

        if(data is null)
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
                StatusCode = StatusCodes.Status201Created
            };


    }
}