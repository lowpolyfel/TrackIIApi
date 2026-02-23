using Microsoft.AspNetCore.Identity;
using Trackii.Api.Contracts;
using Trackii.Api.Interfaces;
using Trackii.Api.Models;

namespace Trackii.Api.Services;

public sealed class AuthService : IAuthService
{
    private const uint DefaultRoleId = 2;
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(IAuthRepository authRepository, IPasswordHasher<User> passwordHasher)
    {
        _authRepository = authRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResponse<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TokenCode))
        {
            return ServiceResponse<RegisterResponse>.Fail("Token es requerido.");
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResponse<RegisterResponse>.Fail("Usuario y contraseña son requeridos.");
        }

        if (string.IsNullOrWhiteSpace(request.DeviceUid))
        {
            return ServiceResponse<RegisterResponse>.Fail("Device UID es requerido.");
        }

        var tokenExists = await _authRepository.TokenExistsAsync(request.TokenCode, cancellationToken);
        if (!tokenExists)
        {
            return ServiceResponse<RegisterResponse>.Fail("Token inválido.", ServiceErrorType.Unauthorized);
        }

        var locationExists = await _authRepository.LocationExistsAsync(request.LocationId, cancellationToken);
        if (!locationExists)
        {
            return ServiceResponse<RegisterResponse>.Fail("Localidad inválida.");
        }

        var normalizedUsername = request.Username.Trim();
        var existingUser = await _authRepository.UsernameExistsAsync(normalizedUsername, cancellationToken);
        if (existingUser)
        {
            return ServiceResponse<RegisterResponse>.Fail("El usuario ya existe.", ServiceErrorType.Conflict);
        }

        var user = new User
        {
            Username = normalizedUsername,
            RoleId = DefaultRoleId,
            Active = true
        };
        user.Password = _passwordHasher.HashPassword(user, request.Password);
        _authRepository.AddUser(user);

        var normalizedUid = request.DeviceUid.Trim();
        var device = await _authRepository.GetDeviceByUidAsync(normalizedUid, cancellationToken);

        if (device is null)
        {
            device = new Device
            {
                DeviceUid = normalizedUid,
                LocationId = request.LocationId,
                Name = string.IsNullOrWhiteSpace(request.DeviceName) ? normalizedUsername : request.DeviceName.Trim(),
                Active = true,
                User = user
            };
            _authRepository.AddDevice(device);
        }
        else
        {
            if (device.UserId.HasValue)
            {
                return ServiceResponse<RegisterResponse>.Fail("El dispositivo ya está asociado a otro usuario.", ServiceErrorType.Conflict);
            }

            device.LocationId = request.LocationId;
            device.User = user;
            device.Name = string.IsNullOrWhiteSpace(request.DeviceName) ? device.Name : request.DeviceName.Trim();
            device.Active = true;
        }

        await _authRepository.SaveChangesAsync(cancellationToken);
        return ServiceResponse<RegisterResponse>.Ok(new RegisterResponse(user.Id, device.Id));
    }

    public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ServiceResponse<LoginResponse>.Fail("Usuario y contraseña son requeridos.");
        }

        if (string.IsNullOrWhiteSpace(request.DeviceUid))
        {
            return ServiceResponse<LoginResponse>.Fail("Device UID es requerido.");
        }

        var user = await _authRepository.GetActiveUserByUsernameAsync(request.Username, cancellationToken);
        if (user is null)
        {
            return ServiceResponse<LoginResponse>.Fail("Credenciales inválidas.", ServiceErrorType.Unauthorized);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return ServiceResponse<LoginResponse>.Fail("Credenciales inválidas.", ServiceErrorType.Unauthorized);
        }

        var device = await _authRepository.GetActiveDeviceByUidForUserAsync(request.DeviceUid, user.Id, cancellationToken);
        if (device is null || device.Location is null)
        {
            return ServiceResponse<LoginResponse>.Fail("Dispositivo no vinculado al usuario.", ServiceErrorType.Unauthorized);
        }

        return ServiceResponse<LoginResponse>.Ok(new LoginResponse(
            user.Id,
            user.Username,
            user.RoleId,
            device.Id,
            device.Name ?? "Dispositivo",
            device.LocationId,
            device.Location.Name));
    }

    public async Task<ServiceResponse<object>> ValidateTokenAsync(string tokenCode, CancellationToken cancellationToken)
    {
        var tokenExists = await _authRepository.TokenExistsAsync(tokenCode, cancellationToken);
        if (!tokenExists)
        {
            return ServiceResponse<object>.Fail("Token inválido.", ServiceErrorType.NotFound);
        }

        return ServiceResponse<object>.Ok(new { isValid = true });
    }
}
