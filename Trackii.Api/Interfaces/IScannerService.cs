using Trackii.Api.Contracts;

namespace Trackii.Api.Interfaces;

public interface IScannerService
{
    Task<ServiceResponse<PartLookupResponse>> GetPartInfoAsync(string partNumber, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkOrderContextResponse>> GetWorkOrderContextAsync(string woNumber, string? partNumber, uint deviceId, CancellationToken cancellationToken);
    Task<ServiceResponse<RegisterScanResponse>> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<ScrapResponse>> ScrapAsync(ScrapRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<ReworkResponse>> ReworkAsync(ReworkRequest request, CancellationToken cancellationToken);
}
