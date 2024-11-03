using System.Text.Json.Serialization;

namespace Heronest.Internal.Api;

public class ApiResponse
{
    public string Status { get; set; }
    public object? Data { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }

    [JsonIgnore] // To prevent it from being serialized
    public Exception? Error { get; set; }
}

public delegate ApiResponse RequestHandler(HttpContext context);

public class ApiHandler
{
    private readonly RequestHandler handler;

    public ApiHandler(RequestHandler handler)
    {
        this.handler = handler;
    }

    public ApiResponse Handle(HttpContext context)
    {
        var response = handler(context);

        context.Response.StatusCode = response.StatusCode;

        return response;
    }
}
