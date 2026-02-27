using Microsoft.EntityFrameworkCore.Storage;
using Trackii.Api.Models;

namespace Trackii.Api.Interfaces;

public interface IScannerRepository
{
    Task<Product?> GetActiveProductWithHierarchyAsync(string partNumber, CancellationToken cancellationToken);
    void AddUnregisteredPart(UnregisteredPart unregisteredPart);

    Task<WorkOrder?> GetWorkOrderContextAsync(string woNumber, CancellationToken cancellationToken);
    Task<Device?> GetActiveDeviceWithLocationAsync(uint deviceId, CancellationToken cancellationToken);
    Task<List<RouteStep>> GetRouteStepsByRouteIdAsync(uint routeId, CancellationToken cancellationToken);
    Task<WipStepExecution?> GetExecutionByWipAndStepAsync(uint wipItemId, uint routeStepId, CancellationToken cancellationToken);
    Task<WipStepExecution?> GetLatestExecutionByWipItemIdAsync(uint wipItemId, CancellationToken cancellationToken);
    Task<Location?> GetLocationByIdAsync(uint locationId, CancellationToken cancellationToken);

    Task<User?> GetActiveUserByIdAsync(uint userId, CancellationToken cancellationToken);
    Task<Device?> GetActiveDeviceByIdAsync(uint deviceId, CancellationToken cancellationToken);
    Task<WorkOrder?> GetWorkOrderForRegisterAsync(string woNumber, CancellationToken cancellationToken);
    Task<Product?> GetActiveProductWithSubfamilyAsync(string partNumber, CancellationToken cancellationToken);
    void AddWorkOrder(WorkOrder workOrder);
    Task<WipItem?> GetWipItemWithExecutionsByWorkOrderIdAsync(uint workOrderId, CancellationToken cancellationToken);
    void AddWipItem(WipItem wipItem);
    void AddWipStepExecution(WipStepExecution execution);
    void AddScanEvent(ScanEvent scanEvent);
    Task<WipItem?> GetWipItemByWorkOrderIdAsync(uint workOrderId, CancellationToken cancellationToken);
    Task<List<ErrorCategory>> GetActiveErrorCategoriesAsync(CancellationToken cancellationToken);
    Task<List<ErrorCode>> GetActiveErrorCodesByCategoryAsync(uint categoryId, CancellationToken cancellationToken);
    Task<ErrorCode?> GetActiveErrorCodeByIdAsync(uint errorCodeId, CancellationToken cancellationToken);
    void AddScrapLog(ScrapLog scrapLog);
    Task ScrapOrderAsync(WorkOrder workOrder, WipItem wipItem, User user, uint errorCodeId, uint quantity, string? comments, CancellationToken cancellationToken);
    Task AddWipReworkLogAsync(WipReworkLog log, CancellationToken cancellationToken);
    Task<WipItem?> GetWipItemByLotNumberAsync(string noLote, CancellationToken cancellationToken);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
