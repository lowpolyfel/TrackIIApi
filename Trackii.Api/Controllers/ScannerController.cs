using Microsoft.AspNetCore.Mvc;
using Trackii.Api.Contracts;
using Trackii.Api.Interfaces;

namespace Trackii.Api.Controllers;

[ApiController]
[Route("api/scanner")]
public sealed class ScannerController : ControllerBase
{
    private readonly IScannerService _scannerService;

    public ScannerController(IScannerService scannerService)
    {
        _scannerService = scannerService;
    }

    [HttpGet("part/{partNumber}")]
    public async Task<IActionResult> GetPartInfo(string partNumber, CancellationToken cancellationToken)
    {
        var response = await _scannerService.GetPartInfoAsync(partNumber, cancellationToken);
        return ToActionResult(response);
    }

    [HttpGet("work-orders/{woNumber}/context")]
    public async Task<IActionResult> GetWorkOrderContext(string woNumber, [FromQuery] uint deviceId, [FromQuery] string? partNumber, CancellationToken cancellationToken)
    {
        var response = await _scannerService.GetWorkOrderContextAsync(woNumber, partNumber, deviceId, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterScan([FromBody] RegisterScanRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Solicitud inválida.");
        }

        var response = await _scannerService.RegisterScanAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("scrap")]
    public async Task<IActionResult> Scrap([FromBody] ScrapRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Solicitud inválida.");
        }

        var response = await _scannerService.ScrapAsync(request, cancellationToken);
        return ToActionResult(response);
    }

    [HttpPost("rework")]
    public async Task<IActionResult> Rework([FromBody] ReworkRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("Solicitud inválida.");
        }

        var response = await _scannerService.ReworkAsync(request, cancellationToken);
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
