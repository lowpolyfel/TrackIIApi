using Trackii.Api.Contracts;
using Trackii.Api.Interfaces;
using Trackii.Api.Models;

namespace Trackii.Api.Services;

public sealed class ScannerService : IScannerService
{
    private readonly IScannerRepository _scannerRepository;

    public ScannerService(IScannerRepository scannerRepository)
    {
        _scannerRepository = scannerRepository;
    }

    public async Task<ServiceResponse<PartLookupResponse>> GetPartInfoAsync(string partNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
        {
            return ServiceResponse<PartLookupResponse>.Fail("Número de parte requerido.");
        }

        var normalized = partNumber.Trim();
        var product = await _scannerRepository.GetActiveProductWithHierarchyAsync(normalized, cancellationToken);

        if (product is null || product.Subfamily is null || product.Subfamily.Family is null || product.Subfamily.Family.Area is null)
        {
            _scannerRepository.AddUnregisteredPart(new UnregisteredPart
            {
                PartNumber = normalized,
                CreationDateTime = DateTime.UtcNow,
                Active = true
            });
            await _scannerRepository.SaveChangesAsync(cancellationToken);

            return ServiceResponse<PartLookupResponse>.Ok(new PartLookupResponse(
                false,
                "El producto no está dado de alta. Contacta a ingeniería.",
                normalized,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null));
        }

        return ServiceResponse<PartLookupResponse>.Ok(new PartLookupResponse(
            true,
            null,
            normalized,
            product.Id,
            product.Subfamily.Id,
            product.Subfamily.Name,
            product.Subfamily.Family.Id,
            product.Subfamily.Family.Name,
            product.Subfamily.Family.Area.Id,
            product.Subfamily.Family.Area.Name,
            product.Subfamily.ActiveRouteId));
    }

    public async Task<ServiceResponse<WorkOrderContextResponse>> GetWorkOrderContextAsync(string woNumber, uint deviceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(woNumber))
        {
            return ServiceResponse<WorkOrderContextResponse>.Fail("Número de orden requerido.");
        }

        var normalizedWorkOrder = woNumber.Trim();
        var workOrder = await _scannerRepository.GetWorkOrderContextAsync(normalizedWorkOrder, cancellationToken);

        if (workOrder is null)
        {
            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(
                IsNew: true,
                PreviousQuantity: 0,
                CurrentStepNumber: 1,
                CurrentStepName: "Paso 1",
                NextSteps: []));
        }

        if (workOrder.Product?.Subfamily?.ActiveRouteId is null)
        {
            return ServiceResponse<WorkOrderContextResponse>.Fail("La orden no tiene ruta activa configurada.");
        }

        var routeSteps = await _scannerRepository.GetRouteStepsByRouteIdAsync(workOrder.Product.Subfamily.ActiveRouteId.Value, cancellationToken);
        if (routeSteps.Count == 0)
        {
            return ServiceResponse<WorkOrderContextResponse>.Fail("La ruta no tiene pasos configurados.");
        }

        var currentStepNumber = 1;
        var currentStepName = routeSteps[0].Location?.Name ?? $"Paso {currentStepNumber}";
        var previousQuantity = 0;
        var nextSteps = routeSteps.Select(step => new NextRouteStepResponse(
                step.Id,
                (int)step.StepNumber,
                step.Location?.Name ?? $"Paso {step.StepNumber}",
                step.LocationId,
                step.Location?.Name ?? $"Location {step.LocationId}"))
            .ToList();

        if (workOrder.WipItem is not null)
        {
            var currentStep = routeSteps.FirstOrDefault(step => step.Id == workOrder.WipItem.CurrentStepId);
            if (currentStep is not null)
            {
                currentStepNumber = (int)currentStep.StepNumber;
                currentStepName = currentStep.Location?.Name ?? $"Paso {currentStep.StepNumber}";
                nextSteps = routeSteps
                    .Where(step => step.StepNumber > currentStep.StepNumber)
                    .Select(step => new NextRouteStepResponse(
                        step.Id,
                        (int)step.StepNumber,
                        step.Location?.Name ?? $"Paso {step.StepNumber}",
                        step.LocationId,
                        step.Location?.Name ?? $"Location {step.LocationId}"))
                    .ToList();
            }

            var latestExecution = await _scannerRepository.GetLatestExecutionByWipItemIdAsync(workOrder.WipItem.Id, cancellationToken);
            if (latestExecution is not null)
            {
                previousQuantity = (int)latestExecution.QtyIn;
            }
        }

        return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(
            IsNew: workOrder.WipItem is null,
            PreviousQuantity: previousQuantity,
            CurrentStepNumber: currentStepNumber,
            CurrentStepName: currentStepName,
            NextSteps: nextSteps));
    }

    public async Task<ServiceResponse<RegisterScanResponse>> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber) || string.IsNullOrWhiteSpace(request.PartNumber))
        {
            return ServiceResponse<RegisterScanResponse>.Fail("Orden y número de parte son requeridos.");
        }

        if (request.Quantity <= 0)
        {
            return ServiceResponse<RegisterScanResponse>.Fail("Cantidad inválida.");
        }

        await using var transaction = await _scannerRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _scannerRepository.GetActiveUserByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("Usuario inválido.", ServiceErrorType.Unauthorized);
            }

            var device = await _scannerRepository.GetActiveDeviceByIdAsync(request.DeviceId, cancellationToken);
            if (device is null || device.UserId != user.Id)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("Dispositivo inválido.", ServiceErrorType.Unauthorized);
            }

            var workOrderNumber = request.WorkOrderNumber.Trim();
            var partNumber = request.PartNumber.Trim();

            var product = await _scannerRepository.GetActiveProductWithSubfamilyAsync(partNumber, cancellationToken);
            if (product?.Subfamily is null)
            {
                _scannerRepository.AddUnregisteredPart(new UnregisteredPart
                {
                    PartNumber = partNumber,
                    CreationDateTime = DateTime.UtcNow,
                    Active = true
                });
                await _scannerRepository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ServiceResponse<RegisterScanResponse>.Fail("Parte no registrada");
            }

            var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(workOrderNumber, cancellationToken);
            var wipItem = workOrder is null
                ? null
                : await _scannerRepository.GetWipItemWithExecutionsByWorkOrderIdAsync(workOrder.Id, cancellationToken);

            if (workOrder is null)
            {
               

                workOrder = new WorkOrder
                {
                    WoNumber = workOrderNumber,
                    ProductId = product.Id,
                    Status = WorkOrderStatus.InProgress.ToDatabaseValue()
                };
                _scannerRepository.AddWorkOrder(workOrder);
                await _scannerRepository.SaveChangesAsync(cancellationToken);
            }

            if (workOrder.Status.IsOneOf(WorkOrderStatus.Cancelled, WorkOrderStatus.Finished))
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La orden no permite avanzar.");
            }

            if (!string.Equals(product.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase) || workOrder.ProductId != product.Id)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("El número de parte no corresponde a la orden.");
            }

            var routeId = product.Subfamily.ActiveRouteId;
            if (routeId is null)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La subfamilia no tiene ruta activa.");
            }

            var steps = await _scannerRepository.GetRouteStepsByRouteIdAsync(routeId.Value, cancellationToken);
            if (steps.Count == 0)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La ruta no tiene pasos configurados.");
            }

            if (wipItem is not null && wipItem.Status.IsOneOf(WipItemStatus.Finished, WipItemStatus.Scrapped, WipItemStatus.Hold))
            {
                return ServiceResponse<RegisterScanResponse>.Fail("El WIP no permite avanzar.");
            }

            var isNew = wipItem is null;
            RouteStep targetStep;
            if (isNew)
            {
                targetStep = steps.First();
            }
            else
            {
                var currentStep = steps.FirstOrDefault(step => step.Id == wipItem!.CurrentStepId);
                if (currentStep is null)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("Paso actual inválido.");
                }

                targetStep = steps.FirstOrDefault(step => step.StepNumber == currentStep.StepNumber + 1)!;
                if (targetStep is null)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("La orden ya está en el último paso.");
                }
            }

            if (targetStep.LocationId != device.LocationId)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("El dispositivo no corresponde al paso actual.");
            }

            if (isNew)
            {
                wipItem = new WipItem
                {
                    WorkOrderId = workOrder.Id,
                    CurrentStepId = targetStep.Id,
                    Status = WipItemStatus.Active.ToDatabaseValue(),
                    CreatedAt = DateTime.UtcNow,
                    RouteId = routeId.Value
                };
                _scannerRepository.AddWipItem(wipItem);
            }
            else
            {
                wipItem!.CurrentStepId = targetStep.Id;
            }

            _scannerRepository.AddWipStepExecution(new WipStepExecution
            {
                WipItem = wipItem!,
                RouteStepId = targetStep.Id,
                UserId = user.Id,
                DeviceId = device.Id,
                LocationId = device.LocationId,
                CreatedAt = DateTime.UtcNow,
                QtyIn = (uint)request.Quantity,
                QtyScrap = 0
            });

            _scannerRepository.AddScanEvent(new ScanEvent
            {
                WipItem = wipItem,
                RouteStepId = targetStep.Id,
                ScanType = ScanType.Entry.ToDatabaseValue(),
                Ts = DateTime.UtcNow
            });

            var isFinalStep = targetStep.StepNumber == steps.Max(step => step.StepNumber);
            if (isFinalStep)
            {
                wipItem!.Status = WipItemStatus.Finished.ToDatabaseValue();
                workOrder.Status = WorkOrderStatus.Finished.ToDatabaseValue();
                _scannerRepository.AddScanEvent(new ScanEvent
                {
                    WipItem = wipItem,
                    RouteStepId = targetStep.Id,
                    ScanType = ScanType.Exit.ToDatabaseValue(),
                    Ts = DateTime.UtcNow
                });
            }

            await _scannerRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResponse<RegisterScanResponse>.Ok(new RegisterScanResponse("Registro completado.", workOrder.Id, wipItem!.Id, targetStep.Id, isFinalStep));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ServiceResponse<ScrapResponse>> ScrapAsync(ScrapRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber))
        {
            return ServiceResponse<ScrapResponse>.Fail("Orden requerida.");
        }

        await using var transaction = await _scannerRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _scannerRepository.GetActiveUserByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return ServiceResponse<ScrapResponse>.Fail("Usuario inválido.", ServiceErrorType.Unauthorized);
            }

            var device = await _scannerRepository.GetActiveDeviceByIdAsync(request.DeviceId, cancellationToken);
            if (device is null || device.UserId != user.Id)
            {
                return ServiceResponse<ScrapResponse>.Fail("Dispositivo inválido.", ServiceErrorType.Unauthorized);
            }

            var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(request.WorkOrderNumber.Trim(), cancellationToken);
            if (workOrder is null)
            {
                return ServiceResponse<ScrapResponse>.Fail("Orden no encontrada.");
            }

            workOrder.Status = WorkOrderStatus.Cancelled.ToDatabaseValue();

            var wipItem = await _scannerRepository.GetWipItemByWorkOrderIdAsync(workOrder.Id, cancellationToken);
            if (wipItem is not null)
            {
                wipItem.Status = WipItemStatus.Scrapped.ToDatabaseValue();
                _scannerRepository.AddScanEvent(new ScanEvent
                {
                    WipItemId = wipItem.Id,
                    RouteStepId = wipItem.CurrentStepId,
                    ScanType = ScanType.Error.ToDatabaseValue(),
                    Ts = DateTime.UtcNow
                });
            }

            await _scannerRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResponse<ScrapResponse>.Ok(new ScrapResponse("Orden cancelada.", workOrder.Id, wipItem?.Id));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ServiceResponse<ReworkResponse>> ReworkAsync(ReworkRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber))
        {
            return ServiceResponse<ReworkResponse>.Fail("Orden requerida.");
        }

        if (request.Quantity == 0)
        {
            return ServiceResponse<ReworkResponse>.Fail("Cantidad inválida.");
        }

        await using var transaction = await _scannerRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _scannerRepository.GetActiveUserByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return ServiceResponse<ReworkResponse>.Fail("Usuario inválido.", ServiceErrorType.Unauthorized);
            }

            var device = await _scannerRepository.GetActiveDeviceByIdAsync(request.DeviceId, cancellationToken);
            if (device is null || device.UserId != user.Id)
            {
                return ServiceResponse<ReworkResponse>.Fail("Dispositivo inválido.", ServiceErrorType.Unauthorized);
            }

            var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(request.WorkOrderNumber.Trim(), cancellationToken);
            if (workOrder is null)
            {
                return ServiceResponse<ReworkResponse>.Fail("Orden no encontrada.");
            }

            var wipItem = await _scannerRepository.GetWipItemByWorkOrderIdAsync(workOrder.Id, cancellationToken);
            if (wipItem is null)
            {
                return ServiceResponse<ReworkResponse>.Fail("WIP no encontrado.");
            }

            _scannerRepository.AddReworkLog(new WipReworkLog
            {
                WipItemId = wipItem.Id,
                LocationId = device.LocationId,
                UserId = user.Id,
                DeviceId = device.Id,
                Qty = request.Quantity,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow
            });

            wipItem.Status = request.Completed ? WipItemStatus.Active.ToDatabaseValue() : WipItemStatus.Hold.ToDatabaseValue();
            await _scannerRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResponse<ReworkResponse>.Ok(new ReworkResponse(request.Completed ? "Rework terminado." : "Rework registrado.", workOrder.Id, wipItem.Id, wipItem.Status));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static bool IsAlloyTabletAllowed(Device device, Product product)
    {
        var deviceLocation = device.Location?.Name?.Trim();
        var isAlloyDevice = string.Equals(deviceLocation, "Alloy", StringComparison.OrdinalIgnoreCase);

        var subfamilyName = product.Subfamily?.Name ?? string.Empty;
        var familyName = product.Subfamily?.Family?.Name ?? string.Empty;

        var isTabletProduct = subfamilyName.Contains("tablet", StringComparison.OrdinalIgnoreCase)
                              || familyName.Contains("tablet", StringComparison.OrdinalIgnoreCase);

        return isAlloyDevice && isTabletProduct;
    }
}
