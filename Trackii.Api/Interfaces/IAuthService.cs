using Trackii.Api.Contracts;

namespace Trackii.Api.Interfaces;

public interface IAuthService
{
    Task<ServiceResponse<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<object>> ValidateTokenAsync(string tokenCode, CancellationToken cancellationToken);
}
