using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Trackii.Api.Data;
using Trackii.Api.Interfaces;
using Trackii.Api.Models;

namespace Trackii.Api.Repositories;

public sealed class ScannerRepository : IScannerRepository
{
    private readonly TrackiiDbContext _dbContext;

    public ScannerRepository(TrackiiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetActiveProductWithHierarchyAsync(string partNumber, CancellationToken cancellationToken) =>
        _dbContext.Products
            .Include(p => p.Subfamily)
            .ThenInclude(sf => sf!.Family)
            .ThenInclude(f => f!.Area)
            .FirstOrDefaultAsync(p => p.PartNumber == partNumber && p.Active, cancellationToken);

    public void AddUnregisteredPart(UnregisteredPart unregisteredPart) => _dbContext.UnregisteredParts.Add(unregisteredPart);

    public Task<WorkOrder?> GetWorkOrderContextAsync(string woNumber, CancellationToken cancellationToken) =>
        _dbContext.WorkOrders
            .Include(wo => wo.Product)
            .ThenInclude(p => p!.Subfamily)
            .ThenInclude(sf => sf!.ActiveRoute)
            .Include(wo => wo.WipItem)
            .ThenInclude(wip => wip!.CurrentStep)
            .ThenInclude(step => step!.Location)
            .FirstOrDefaultAsync(wo => wo.WoNumber == woNumber, cancellationToken);

    public Task<Device?> GetActiveDeviceWithLocationAsync(uint deviceId, CancellationToken cancellationToken) =>
        _dbContext.Devices
            .Include(d => d.Location)
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.Active, cancellationToken);

    public Task<List<RouteStep>> GetRouteStepsByRouteIdAsync(uint routeId, CancellationToken cancellationToken) =>
        _dbContext.RouteSteps
            .Include(step => step.Location)
            .Where(step => step.RouteId == routeId)
            .OrderBy(step => step.StepNumber)
            .ToListAsync(cancellationToken);

    public Task<WipStepExecution?> GetExecutionByWipAndStepAsync(uint wipItemId, uint routeStepId, CancellationToken cancellationToken) =>
        _dbContext.WipStepExecutions
            .FirstOrDefaultAsync(exec => exec.WipItemId == wipItemId && exec.RouteStepId == routeStepId, cancellationToken);

    public Task<WipStepExecution?> GetLatestExecutionByWipItemIdAsync(uint wipItemId, CancellationToken cancellationToken) =>
        _dbContext.WipStepExecutions
            .Where(exec => exec.WipItemId == wipItemId)
            .OrderByDescending(exec => exec.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Location?> GetLocationByIdAsync(uint locationId, CancellationToken cancellationToken) =>
        _dbContext.Locations.FirstOrDefaultAsync(location => location.Id == locationId, cancellationToken);

    public Task<User?> GetActiveUserByIdAsync(uint userId, CancellationToken cancellationToken) =>
        _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Active, cancellationToken);

    public Task<Device?> GetActiveDeviceByIdAsync(uint deviceId, CancellationToken cancellationToken) =>
        _dbContext.Devices.Include(d => d.Location).FirstOrDefaultAsync(d => d.Id == deviceId && d.Active, cancellationToken);

    public Task<WorkOrder?> GetWorkOrderForRegisterAsync(string woNumber, CancellationToken cancellationToken) =>
        _dbContext.WorkOrders
            .Include(wo => wo.Product)
            .ThenInclude(p => p!.Subfamily)
            .FirstOrDefaultAsync(wo => wo.WoNumber == woNumber, cancellationToken);

    public Task<Product?> GetActiveProductWithSubfamilyAsync(string partNumber, CancellationToken cancellationToken) =>
        _dbContext.Products
            .Include(p => p.Subfamily)
            .ThenInclude(sf => sf!.Family)
            .ThenInclude(f => f!.Area)
            .Include(p => p.Subfamily)
            .ThenInclude(sf => sf!.ActiveRoute)
            .FirstOrDefaultAsync(p => p.PartNumber == partNumber && p.Active, cancellationToken);

    public void AddWorkOrder(WorkOrder workOrder) => _dbContext.WorkOrders.Add(workOrder);

    public Task<WipItem?> GetWipItemWithExecutionsByWorkOrderIdAsync(uint workOrderId, CancellationToken cancellationToken) =>
        _dbContext.WipItems.Include(wip => wip.StepExecutions).FirstOrDefaultAsync(wip => wip.WorkOrderId == workOrderId, cancellationToken);

    public void AddWipItem(WipItem wipItem) => _dbContext.WipItems.Add(wipItem);

    public void AddWipStepExecution(WipStepExecution execution) => _dbContext.WipStepExecutions.Add(execution);

    public void AddScanEvent(ScanEvent scanEvent) => _dbContext.ScanEvents.Add(scanEvent);

    public Task<WipItem?> GetWipItemByWorkOrderIdAsync(uint workOrderId, CancellationToken cancellationToken) =>
        _dbContext.WipItems.FirstOrDefaultAsync(wip => wip.WorkOrderId == workOrderId, cancellationToken);


    public Task<List<ErrorCategory>> GetActiveErrorCategoriesAsync(CancellationToken cancellationToken) =>
        _dbContext.ErrorCategories
            .Where(category => category.Active)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

    public Task<List<ErrorCode>> GetActiveErrorCodesByCategoryAsync(uint categoryId, CancellationToken cancellationToken) =>
        _dbContext.ErrorCodes
            .Where(code => code.CategoryId == categoryId && code.Active)
            .OrderBy(code => code.Code)
            .ToListAsync(cancellationToken);

    public Task<ErrorCode?> GetActiveErrorCodeByIdAsync(uint errorCodeId, CancellationToken cancellationToken) =>
        _dbContext.ErrorCodes
            .FirstOrDefaultAsync(code => code.Id == errorCodeId && code.Active, cancellationToken);

    public void AddScrapLog(ScrapLog scrapLog) => _dbContext.ScrapLogs.Add(scrapLog);

    public async Task ScrapOrderAsync(WorkOrder workOrder, WipItem wipItem, User user, uint errorCodeId, uint quantity, string? comments, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _dbContext.ScrapLogs.Add(new ScrapLog
            {
                WipItemId = wipItem.Id,
                ErrorCodeId = errorCodeId,
                RouteStepId = wipItem.CurrentStepId,
                UserId = user.Id,
                Qty = quantity,
                Comments = comments,
                CreatedAt = DateTime.UtcNow
            });

            wipItem.Status = WipItemStatus.Scrapped.ToDatabaseValue();
            workOrder.Status = WorkOrderStatus.Cancelled.ToDatabaseValue();

            _dbContext.ScanEvents.Add(new ScanEvent
            {
                WipItemId = wipItem.Id,
                RouteStepId = wipItem.CurrentStepId,
                ScanType = ScanType.Exit.ToDatabaseValue(),
                Ts = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void AddReworkLog(WipReworkLog log) => _dbContext.WipReworkLogs.Add(log);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        _dbContext.Database.BeginTransactionAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _dbContext.SaveChangesAsync(cancellationToken);
}
