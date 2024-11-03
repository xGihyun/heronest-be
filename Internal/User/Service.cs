using Heronest.Internal.Api;

namespace Heronest.Internal.User;

public class UserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository userRepository)
    {
        _repository = userRepository;
    }

    public async Task<ApiResponse> HandleGetById(HttpContext httpContext, Guid userId)
    {
        UserResponse user = await _repository.GetById(userId);

        return new ApiResponse
        {
            Status = "success",
            StatusCode = StatusCodes.Status200OK,
            Data = user,
        };
    }
}
