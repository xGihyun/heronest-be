using Heronest.Internal.Api;

namespace Heronest.Internal.Ticket;

public class TicketController
{
    private readonly ITicketRepository repository;

    public TicketController(ITicketRepository ticketRepository)
    {
        this.repository = ticketRepository;
    }

    public async Task<ApiResponse> CreateTicket(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<TicketRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }
        await this.repository.CreateTicket(data);
        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "API response success",
        };
    }
}

