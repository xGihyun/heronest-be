using Heronest.Features.Api;
using Npgsql;

namespace Heronest.Features.Ticket;

public class TicketController
{
    private readonly ITicketRepository repository;

    public TicketController(ITicketRepository ticketRepository)
    {
        this.repository = ticketRepository;
    }

    public async Task<ApiResponse> GetByTicketNumber(HttpContext context)
    {
        string? ticketNumber = context.GetRouteValue("ticketNumber")?.ToString();

        if (ticketNumber is null)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Fail,
                StatusCode = StatusCodes.Status404NotFound,
                Message = "Ticket number not found.",
            };
        }

        try
        {
            var ticket = await this.repository.GetByTicketNumber(ticketNumber);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully fetched ticket.",
                Data = ticket,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to fetch ticket.",
                Error = ex,
            };
        }
    }

    public async Task<ApiResponse> GetMany(HttpContext context)
    {
        Guid? userId = QueryParameter.TryGetValueFromStruct<Guid>(context, "userId");
        Guid? eventId = QueryParameter.TryGetValueFromStruct<Guid>(context, "eventId");

        try
        {
            var pagination = new PaginationQuery(context);
            var (offset, limit) = pagination.GetValues();

            var filter = new GetTicketFilter(offset, limit, eventId, userId);
            var tickets = await this.repository.GetMany(filter);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status200OK,
                Message = "Successfully fetched tickets.",
                Data = tickets,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to fetch tickets.",
                Data = new Ticket[] { },
                Error = ex,
            };
        }
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
                Message = "Invalid JSON request.",
            };
        }

        try
        {
            var ticket = await this.repository.Create(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully created ticket.",
                Data = ticket,
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
                Message = "Failed to create ticket.",
                Error = ex,
            };
        }
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

        try
        {
            await this.repository.Update(data);

            return new ApiResponse
            {
                Status = ApiResponseStatus.Success,
                StatusCode = StatusCodes.Status201Created,
                Message = "Successfully updated.",
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse
            {
                Status = ApiResponseStatus.Error,
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Failed to update ticket.",
                Error = ex,
            };
        }
    }
}
