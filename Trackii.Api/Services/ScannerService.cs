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

        var workOrder = await _scannerRepository.GetWorkOrderContextAsync(woNumber, cancellationToken);

        if (workOrder is null || workOrder.Product is null || workOrder.Product.Subfamily is null)
        {
            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(false, "Orden no encontrada.", null, null, null, null, null, null, null, null, null, null, null, null, false, false));
        }

        var routeId = workOrder.Product.Subfamily.ActiveRouteId;
        if (routeId is null)
        {
            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(true, "La subfamilia no tiene ruta activa.", workOrder.Id, workOrder.Status, workOrder.ProductId, workOrder.Product.PartNumber, null, null, null, null, null, null, null, null, false, false));
        }

        var device = await _scannerRepository.GetActiveDeviceWithLocationAsync(deviceId, cancellationToken);
        if (device is null || device.Location is null)
        {
            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(true, "Dispositivo inválido.", workOrder.Id, workOrder.Status, workOrder.ProductId, workOrder.Product.PartNumber, routeId, null, null, null, null, null, null, null, false, false));
        }

        var steps = await _scannerRepository.GetRouteStepsByRouteIdAsync(routeId.Value, cancellationToken);
        if (steps.Count == 0)
        {
            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(true, "La ruta no tiene pasos configurados.", workOrder.Id, workOrder.Status, workOrder.ProductId, workOrder.Product.PartNumber, routeId, null, null, null, null, null, null, null, false, false));
        }

        var isFirstStep = workOrder.WipItem is null;
        uint? previousQty = null;
        uint? maxQty = null;
        RouteStep? nextStep;
        RouteStep? currentStep = null;

        if (isFirstStep)
        {
            nextStep = steps.First();
        }
        else
        {
            currentStep = steps.FirstOrDefault(step => step.Id == workOrder.WipItem!.CurrentStepId);
            if (currentStep is null)
            {
                return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(true, "Paso actual inválido.", workOrder.Id, workOrder.Status, workOrder.ProductId, workOrder.Product.PartNumber, routeId, workOrder.WipItem!.CurrentStepId, null, null, null, null, null, null, false, false));
            }

            nextStep = steps.FirstOrDefault(step => step.StepNumber == currentStep.StepNumber + 1);
            var previousExecution = await _scannerRepository.GetExecutionByWipAndStepAsync(workOrder.WipItem!.Id, currentStep.Id, cancellationToken);
            if (previousExecution is not null)
            {
                previousQty = previousExecution.QtyIn;
                maxQty = previousExecution.QtyIn;
            }
        }

        if (nextStep is null)
        {
            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(true, "La orden ya está en el último paso.", workOrder.Id, workOrder.Status, workOrder.ProductId, workOrder.Product.PartNumber, routeId, currentStep?.Id, null, null, null, null, previousQty, maxQty, isFirstStep, false));
        }

        var nextLocation = await _scannerRepository.GetLocationByIdAsync(nextStep.LocationId, cancellationToken);
        var isOnRework = workOrder.WipItem is not null && workOrder.WipItem.Status == "HOLD";
        var canProceed = !isOnRework && workOrder.Status != "CANCELLED" && workOrder.Status != "FINISHED";

        return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(
            true,
            canProceed ? null : isOnRework ? "El WIP está en rework." : "La orden no permite avanzar.",
            workOrder.Id,
            workOrder.Status,
            workOrder.ProductId,
            workOrder.Product.PartNumber,
            routeId,
            currentStep?.Id,
            nextStep.Id,
            nextStep.StepNumber,
            nextStep.LocationId,
            nextLocation?.Name,
            previousQty,
            maxQty,
            isFirstStep,
            canProceed));
    }

    public async Task<ServiceResponse<RegisterScanResponse>> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber) || string.IsNullOrWhiteSpace(request.PartNumber))
        {
            return ServiceResponse<RegisterScanResponse>.Fail("Orden y número de parte son requeridos.");
        }

        if (request.Quantity == 0)
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

            async Task LogSkipStepAsync(uint wipItemId, uint routeStepId)
            {
                _scannerRepository.AddScanEvent(new ScanEvent
                {
                    WipItemId = wipItemId,
                    RouteStepId = routeStepId,
                    ScanType = "ERROR",
                    Ts = DateTime.UtcNow
                });
                await _scannerRepository.SaveChangesAsync(cancellationToken);
            }

            var workOrderNumber = request.WorkOrderNumber.Trim();
            var partNumber = request.PartNumber.Trim();
            var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(workOrderNumber, cancellationToken);

            if (workOrder is null)
            {
                var canCreateWorkOrder = device.LocationId == 1 || (device.Location?.Name is not null && device.Location.Name.Equals("Alloy", StringComparison.OrdinalIgnoreCase));
                if (!canCreateWorkOrder)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("Orden no encontrada.");
                }

                var product = await _scannerRepository.GetActiveProductWithSubfamilyAsync(partNumber, cancellationToken);
                if (product is null || product.Subfamily is null)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("Producto no encontrado para crear la orden.");
                }

                workOrder = new WorkOrder
                {
                    WoNumber = workOrderNumber,
                    ProductId = product.Id,
                    Status = "OPEN"
                };
                _scannerRepository.AddWorkOrder(workOrder);
                await _scannerRepository.SaveChangesAsync(cancellationToken);
                workOrder.Product = product;
            }

            if (workOrder.Product is null || workOrder.Product.Subfamily is null)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("Orden no encontrada.");
            }

            if (!workOrder.Product.Active)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("El producto no está activo.");
            }

            if (!string.Equals(workOrder.Product.PartNumber, partNumber, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResponse<RegisterScanResponse>.Fail("El número de parte no corresponde a la orden.");
            }

            if (workOrder.Status == "CANCELLED")
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La orden está cancelada.");
            }

            if (workOrder.Status == "FINISHED")
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La orden ya está finalizada.");
            }

            var routeId = workOrder.Product.Subfamily.ActiveRouteId;
            if (routeId is null)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La subfamilia no tiene ruta activa.");
            }

            var steps = await _scannerRepository.GetRouteStepsByRouteIdAsync(routeId.Value, cancellationToken);
            if (steps.Count == 0)
            {
                return ServiceResponse<RegisterScanResponse>.Fail("La ruta no tiene pasos configurados.");
            }

            var wipItem = await _scannerRepository.GetWipItemWithExecutionsByWorkOrderIdAsync(workOrder.Id, cancellationToken);
            if (wipItem is not null && wipItem.Status != "ACTIVE")
            {
                return ServiceResponse<RegisterScanResponse>.Fail(wipItem.Status == "HOLD" ? "El WIP está en rework." : "El WIP no está activo.");
            }

            RouteStep currentStep;
            RouteStep targetStep;
            var isFirstStep = wipItem is null;

            if (isFirstStep)
            {
                targetStep = steps.First();
            }
            else
            {
                currentStep = steps.FirstOrDefault(step => step.Id == wipItem!.CurrentStepId)!;
                if (currentStep is null)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("Paso actual inválido.");
                }

                targetStep = steps.FirstOrDefault(step => step.StepNumber == currentStep.StepNumber + 1)!;
                if (targetStep is null)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("La orden ya está en el último paso.");
                }

                var previousExecution = wipItem!.StepExecutions.FirstOrDefault(exec => exec.RouteStepId == currentStep.Id);
                if (previousExecution is null)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("No se encontró cantidad previa.");
                }

                if (request.Quantity > previousExecution.QtyIn)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("Cantidad mayor a la permitida.");
                }
            }

            if (targetStep.LocationId != device.LocationId)
            {
                if (wipItem is not null)
                {
                    var attemptedStep = steps.FirstOrDefault(step => step.LocationId == device.LocationId) ?? targetStep;
                    await LogSkipStepAsync(wipItem.Id, attemptedStep.Id);
                }

                return ServiceResponse<RegisterScanResponse>.Fail("El dispositivo no corresponde al paso actual.");
            }

            if (wipItem is not null && wipItem.StepExecutions.Any(exec => exec.RouteStepId == targetStep.Id))
            {
                await LogSkipStepAsync(wipItem.Id, targetStep.Id);
                return ServiceResponse<RegisterScanResponse>.Fail("El paso ya fue registrado.");
            }

            if (isFirstStep)
            {
                wipItem = new WipItem
                {
                    WorkOrderId = workOrder.Id,
                    CurrentStepId = targetStep.Id,
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow,
                    RouteId = routeId.Value
                };
                _scannerRepository.AddWipItem(wipItem);
                if (workOrder.Status == "OPEN")
                {
                    workOrder.Status = "IN_PROGRESS";
                }
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
                QtyIn = request.Quantity,
                QtyScrap = 0
            });

            _scannerRepository.AddScanEvent(new ScanEvent
            {
                WipItem = wipItem,
                RouteStepId = targetStep.Id,
                ScanType = "ENTRY",
                Ts = DateTime.UtcNow
            });

            var isFinalStep = targetStep.StepNumber == steps.Max(step => step.StepNumber);
            if (isFinalStep)
            {
                wipItem.Status = "FINISHED";
                workOrder.Status = "FINISHED";
            }

            await _scannerRepository.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ServiceResponse<RegisterScanResponse>.Ok(new RegisterScanResponse("Registro completado.", workOrder.Id, wipItem.Id, targetStep.Id, isFinalStep));
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

            workOrder.Status = "CANCELLED";

            var wipItem = await _scannerRepository.GetWipItemByWorkOrderIdAsync(workOrder.Id, cancellationToken);
            if (wipItem is not null)
            {
                wipItem.Status = "SCRAPPED";
                _scannerRepository.AddScanEvent(new ScanEvent
                {
                    WipItemId = wipItem.Id,
                    RouteStepId = wipItem.CurrentStepId,
                    ScanType = "ERROR",
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

            wipItem.Status = request.Completed ? "ACTIVE" : "HOLD";
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
}
