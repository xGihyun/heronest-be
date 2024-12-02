using Heronest.Internal.Api;

namespace Heronest.Internal.Ticket;

public class TicketController
{
    private readonly ITicketRepository repository;

    public TicketController(ITicketRepository ticketRepository)
    {
        this.repository = ticketRepository;
    }

    public async Task<ApiResponse> Get(HttpContext context)
    {
        var tickets = await this.repository.Get();

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status200OK,
            Message = "Successfully fetched tickets.",
            Data = tickets,
        };
    }

    public async Task<ApiResponse> Create(HttpContext context)
    {
        var data = await context.Request.ReadFromJsonAsync<CreateTicketRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request."
            };
        }

        var result = await this.repository.Create(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully created ticket.",
            Data = result,
        };
    }

    public async Task<ApiResponse> Update(HttpContext context)
    {
        Guid ticketId;

        if (!Guid.TryParse(context.GetRouteValue("ticketId")?.ToString(), out ticketId))
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid ticket ID.",
            };
        }

        var data = await context.Request.ReadFromJsonAsync<UpdateTicketRequest>();

        if (data is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Invalid JSON request.",
            };
        }

        if (data.TicketId != ticketId)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Ticket IDs don't match.",
            };
        }

        await this.repository.Update(data);

        return new ApiResponse
        {
            Status = ApiResponseStatus.Success,
            StatusCode = StatusCodes.Status201Created,
            Message = "Successfully updated.",
        };
    }
}
