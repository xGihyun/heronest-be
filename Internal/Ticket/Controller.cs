using Heronest.Internal.Api;

namespace Heronest.Internal.Ticket;

public class TicketController
{
    private readonly ITicketRepository repository;

    public TicketController(ITicketRepository ticketRepository)
    {
        this.repository = ticketRepository;
    }

    // public async Task<ApiResponse> CreateRequest(HttpContext context)
    // {
    //     var data = await context.Request.ReadFromJsonAsync<CreateTicketRequest>();

    //     if (data is null)
    //     {
    //         return new ApiResponse
    //         {
    //             Status = ApiResponseStatus.Fail,
    //             StatusCode = StatusCodes.Status400BadRequest,
    //         };
    //     }
    //     await this.repository.Create(data);
    //     return new ApiResponse
    //     {
    //         Status = ApiResponseStatus.Success,
    //         StatusCode = StatusCodes.Status201Created,
    //         Message = "Created Successfully",
    //     };
    // }

    // ticket response
    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateTicketResponse>();

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
            Message = "Created Successfully",
        };
    }

    // Update: 
    public async Task<ApiResponse> UpdateTicket(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<UpdateTicketRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }
        await this.repository.Update(data);
        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Created Successfully",
        };
    }
}

