using System.Text.Json.Serialization;

namespace Heronest.Features.Api;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApiResponseStatus
{
    Success,
    Fail,
    Error,
}

public class ApiResponse
{
    public ApiResponseStatus Status { get; set; }
    public object? Data { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }

    [JsonIgnore]
    public Exception? Error { get; set; }
}

public delegate Task<ApiResponse> ApiEndpointHandler(HttpContext context);

public class ApiHandler
{
    public static Delegate Handle(ApiEndpointHandler handler)
    {
        return async Task<IResult> (HttpContext context) =>
        {
            try
            {
                var response = await handler(context);

                if (response.Error is not null)
                {
                    response.Status = ApiResponseStatus.Error;
                    response.Message = $"{response.Message} - {response.Error.Message}";
                    return Results.Json(response, statusCode: response.StatusCode);
                }

                Console.WriteLine(response.Message);

                return Results.Json(response, statusCode: response.StatusCode);
            }
            catch (Exception ex)
            {
                // TODO: I probably don't need this.
                var response = new ApiResponse
                {
                    Status = ApiResponseStatus.Error,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = ex.Message,
                };

                Console.WriteLine("Unhandled: ", ex);

                return Results.Json(response, statusCode: response.StatusCode);
            }
        };
    }
}


/*public static class ApiHandler*/
/*{*/
/*    public static Func<HttpContext, Task<IResult>> Handle(Func<HttpContext, Task<ApiResponse>> handler) */
/*    {*/
/*        return async (HttpContext context) =>*/
/*        {*/
/*            try*/
/*            {*/
/*                var response = await handler(context);*/
/*                return Results.Json(response, statusCode: response.StatusCode);*/
/*            }*/
/*            catch (Exception ex)*/
/*            {*/
/*                var response = new ApiResponse*/
/*                {*/
/*                    Status = ApiResponseStatus.Error,*/
/*                    StatusCode = StatusCodes.Status500InternalServerError,*/
/*                    Message = ex.Message,*/
/*                };*/
/*                return Results.Json(response, statusCode: response.StatusCode);*/
/*            }*/
/*        };*/
/*    }*/
/*}*/
