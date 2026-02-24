using Microsoft.AspNetCore.Mvc;
using Trackii.Api.Contracts;
using Trackii.Api.Interfaces;

namespace Trackii.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Solicitud inválida.");
        }

        var response = await _authService.RegisterAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Solicitud inválida.");
        }

        var response = await _authService.LoginAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("validate-token")]
    public async Task<IActionResult> ValidateToken([FromQuery] string tokenCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenCode))
        {
            return BadRequest("Token es requerido.");
        }

        var response = await _authService.ValidateTokenAsync(tokenCode, cancellationToken);
        return ToActionResult(response);
    }

    private IActionResult ToActionResult<T>(ServiceResponse<T> response)
    {
        if (response.Success)
        {
            return Ok(response.Data);
        }

        return response.ErrorType switch
        {
            ServiceErrorType.Unauthorized => Unauthorized(response.Message),
            ServiceErrorType.Conflict => Conflict(response.Message),
            ServiceErrorType.NotFound => NotFound(response.Message),
            _ => BadRequest(response.Message)
        };
    }
}
