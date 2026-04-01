using Trackii.AdminApp.Models;

namespace Trackii.AdminApp.Services;

public interface ITrackiiAdminApiClient
{
    Task<PartLookupResponse?> GetPartInfoAsync(string partNumber, CancellationToken cancellationToken = default);
    Task<WorkOrderContextResponse?> GetWorkOrderContextAsync(string workOrderNumber, string partNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ErrorCategoryResponse>> GetErrorCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ErrorCodeResponse>> GetErrorCodesByCategoryAsync(uint categoryId, CancellationToken cancellationToken = default);
    Task<string> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken = default);
}
