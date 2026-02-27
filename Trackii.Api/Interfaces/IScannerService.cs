using Trackii.Api.Contracts;

namespace Trackii.Api.Interfaces;

public interface IScannerService
{
    Task<ServiceResponse<PartLookupResponse>> GetPartInfoAsync(string partNumber, CancellationToken cancellationToken);
    Task<ServiceResponse<WorkOrderContextResponse>> GetWorkOrderContextAsync(string woNumber, string? partNumber, uint deviceId, CancellationToken cancellationToken);
    Task<ServiceResponse<RegisterScanResponse>> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<IReadOnlyList<ErrorCategoryResponse>>> GetErrorCategoriesAsync(CancellationToken cancellationToken);
    Task<ServiceResponse<IReadOnlyList<ErrorCodeResponse>>> GetErrorCodesByCategoryAsync(uint categoryId, CancellationToken cancellationToken);
    Task<ServiceResponse<ScrapResponse>> ScrapOrderAsync(ScrapOrderRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<ReworkResponse>> ProcessReworkAsync(ReworkRequest request, CancellationToken cancellationToken);
    Task<ServiceResponse<WipItemValidationResponse>> ValidateReworkAsync(string noLote, CancellationToken cancellationToken);
    Task<ServiceResponse<ReleaseWipItemResponse>> ReleaseWipItemAsync(string noLote, CancellationToken cancellationToken);
}
