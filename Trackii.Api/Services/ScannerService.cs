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
                    false,                                                      // 1. Found
                    "El producto no está dado de alta. Contacta a ingeniería.", // 2. Message
                    normalized,                                                 // 3. PartNumber
                    null,                                                       // 4. ProductId
                    null,                                                       // 5. SubfamilyId
                    null,                                                       // 6. SubfamilyName
                    null,                                                       // 7. FamilyId
                    null,                                                       // 8. FamilyName
                    null,                                                       // 9. AreaId
                    null,                                                       // 10. AreaName
                    null,                                                       // 11. ActiveRouteId
                    null,                                                       // 12. CurrentLocationName 
                    null                                                        // 13. NextLocationName
                ));
        }

        string? currentLocationName = null;
        string? nextLocationName = null;

        if (product.Subfamily.ActiveRouteId is not null)
        {
            var routeSteps = await _scannerRepository.GetRouteStepsByRouteIdAsync(product.Subfamily.ActiveRouteId.Value, cancellationToken);
            currentLocationName = routeSteps.FirstOrDefault(step => step.StepNumber == 1)?.Location?.Name;
            nextLocationName = routeSteps.FirstOrDefault(step => step.StepNumber == 2)?.Location?.Name;
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
            product.Subfamily.ActiveRouteId,
            currentLocationName,
            nextLocationName));
    }

    public async Task<ServiceResponse<WorkOrderContextResponse>> GetWorkOrderContextAsync(string woNumber, string? partNumber, uint deviceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(woNumber))
        {
            return ServiceResponse<WorkOrderContextResponse>.Fail("Número de orden requerido.");
        }

        var normalizedWorkOrder = woNumber.Trim();
        var workOrder = await _scannerRepository.GetWorkOrderContextAsync(normalizedWorkOrder, cancellationToken);
        Product? product = workOrder?.Product;
        if (product is null && !string.IsNullOrWhiteSpace(partNumber))
        {
            product = await _scannerRepository.GetActiveProductWithSubfamilyAsync(partNumber.Trim(), cancellationToken);
        }

        var currentStepName = "Paso 1";
        var currentLocationName = "Paso 1";
        IReadOnlyList<NextRouteStepResponse> nextSteps = [];

        if (workOrder is null)
        {
            // OJO AQUÍ: Se quitaron las palabras 'string' y 'var' porque las variables ya existen
            currentStepName = "Paso 1";
            var routeName = product?.Subfamily?.ActiveRoute?.Name ?? product?.Subfamily?.ActiveRouteId?.ToString() ?? "Sin ruta";
            nextSteps = new List<NextRouteStepResponse>(); // Solo se reasigna

            if (product?.Subfamily?.ActiveRouteId is not null)
            {
                // OJO AQUÍ: Cambiamos el nombre a 'pasosRuta' para que no choque con 'routeSteps' de afuera
                var pasosRuta = await _scannerRepository.GetRouteStepsByRouteIdAsync(product.Subfamily.ActiveRouteId.Value, cancellationToken);
                if (pasosRuta.Count > 0)
                {
                    currentStepName = pasosRuta.FirstOrDefault(s => s.StepNumber == 1)?.Location?.Name ?? "Paso 1";
                    currentLocationName = currentStepName;

                    var step2 = pasosRuta.FirstOrDefault(s => s.StepNumber == 2);
                    if (step2 is not null)
                    {
                        // Como 'nextSteps' ya es una lista, usamos .Add
                        (nextSteps as List<NextRouteStepResponse>)?.Add(new NextRouteStepResponse(
                            step2.Id,
                            (int)step2.StepNumber,
                            step2.Location?.Name ?? "Paso 2",
                            step2.LocationId,
                            step2.Location?.Name ?? "Paso 2"
                        ));
                    }
                }
            }

            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(
                IsNew: true,
                PreviousQuantity: 0,
                CurrentStepNumber: 1,
                CurrentStepName: currentStepName,
                CurrentLocationName: currentLocationName,
                RouteName: routeName,
                NextSteps: nextSteps));
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
        currentStepName = routeSteps[0].Location?.Name ?? $"Paso {currentStepNumber}";
        currentLocationName = routeSteps[0].Location?.Name ?? $"Location {routeSteps[0].LocationId}";
        var previousQuantity = 0;
        nextSteps = routeSteps.Select(step => new NextRouteStepResponse(
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
                currentLocationName = currentStep.Location?.Name ?? $"Location {currentStep.LocationId}";
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
            CurrentLocationName: currentLocationName,
            RouteName: workOrder.Product?.Subfamily?.ActiveRoute?.Name,
            NextSteps: nextSteps));
    }

    public async Task<ServiceResponse<RegisterScanResponse>> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber) || string.IsNullOrWhiteSpace(request.PartNumber))
            return ServiceResponse<RegisterScanResponse>.Fail("Orden y número de parte son requeridos.");

        if (request.Quantity <= 0)
            return ServiceResponse<RegisterScanResponse>.Fail("Cantidad inválida.");

        await using var transaction = await _scannerRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = await _scannerRepository.GetActiveUserByIdAsync(request.UserId, cancellationToken);
            if (user is null) return ServiceResponse<RegisterScanResponse>.Fail("Usuario inválido.", ServiceErrorType.Unauthorized);

            var device = await _scannerRepository.GetActiveDeviceByIdAsync(request.DeviceId, cancellationToken);
            if (device is null || device.UserId != user.Id) return ServiceResponse<RegisterScanResponse>.Fail("Dispositivo inválido.", ServiceErrorType.Unauthorized);

            var workOrderNumber = request.WorkOrderNumber.Trim();
            var partNumber = request.PartNumber.Trim();

            // 1. BUSCAR PRODUCTO, SUBFAMILIA Y RUTA PRIMERO
            var product = await _scannerRepository.GetActiveProductWithSubfamilyAsync(partNumber, cancellationToken);
            if (product?.Subfamily is null)
            {
                _scannerRepository.AddUnregisteredPart(new UnregisteredPart
                {
                    PartNumber = partNumber,
                    CreationDateTime = DateTime.UtcNow,
                    Active = true
                });
                _scannerRepository.AddScanEvent(new ScanEvent
                {
                    ScanType = ScanType.Error.ToDatabaseValue(),
                    Ts = DateTime.UtcNow
                });
                await _scannerRepository.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ServiceResponse<RegisterScanResponse>.Fail("Parte no registrada");
            }

            var routeId = product.Subfamily.ActiveRouteId;
            if (routeId is null) return ServiceResponse<RegisterScanResponse>.Fail("La subfamilia no tiene ruta activa.");

            var steps = await _scannerRepository.GetRouteStepsByRouteIdAsync(routeId.Value, cancellationToken);
            if (steps.Count == 0) return ServiceResponse<RegisterScanResponse>.Fail("La ruta no tiene pasos configurados.");

            // 2. BUSCAR ORDEN EXISTENTE Y VALIDAR REGLAS PREVIAS
            var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(workOrderNumber, cancellationToken);
            var wipItem = workOrder is null ? null : await _scannerRepository.GetWipItemWithExecutionsByWorkOrderIdAsync(workOrder.Id, cancellationToken);

            if (workOrder is not null)
            {
                if (workOrder.Status.IsOneOf(WorkOrderStatus.Cancelled, WorkOrderStatus.Finished))
                    return ServiceResponse<RegisterScanResponse>.Fail("La orden no permite avanzar.");

                if (workOrder.ProductId != product.Id)
                    return ServiceResponse<RegisterScanResponse>.Fail("El número de parte no corresponde a la orden.");

                if (wipItem is not null && wipItem.Status.IsOneOf(WipItemStatus.Finished, WipItemStatus.Scrapped, WipItemStatus.Hold))
                    return ServiceResponse<RegisterScanResponse>.Fail("El WIP no permite avanzar.");
            }

            // 3. DEFINIR EL PASO Y VALIDAR LA UBICACIÓN DE LA TABLETA
            var isNew = wipItem is null;
            RouteStep targetStep;

            if (isNew)
            {
                targetStep = steps.First();
                // VALIDACIÓN CLAVE: Si la orden es nueva, la tableta DEBE estar en el Paso 1
                if (targetStep.LocationId != device.LocationId)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail($"Para crear la orden, la tableta debe estar en el primer paso: {targetStep.Location?.Name}");
                }
            }
            else
            {
                var currentStep = steps.FirstOrDefault(step => step.Id == wipItem!.CurrentStepId);
                if (currentStep is null) return ServiceResponse<RegisterScanResponse>.Fail("Paso actual inválido.");

                targetStep = steps.FirstOrDefault(step => step.StepNumber == currentStep.StepNumber + 1)!;
                if (targetStep is null) return ServiceResponse<RegisterScanResponse>.Fail("La orden ya está en el último paso.");

                if (targetStep.LocationId != device.LocationId)
                    return ServiceResponse<RegisterScanResponse>.Fail("El dispositivo no corresponde al paso actual.");

                // Validar cantidad
                var latestExecution = await _scannerRepository.GetLatestExecutionByWipItemIdAsync(wipItem!.Id, cancellationToken);
                if (latestExecution is not null && request.Quantity > latestExecution.QtyIn)
                    return ServiceResponse<RegisterScanResponse>.Fail($"La cantidad ({request.Quantity}) supera el paso anterior ({latestExecution.QtyIn}).");
            }

            // 4. TODO ESTÁ VÁLIDO -> AHORA SÍ, INSERTAMOS EN BASE DE DATOS
            if (workOrder is null)
            {
                workOrder = new WorkOrder
                {
                    WoNumber = workOrderNumber,
                    ProductId = product.Id,
                    Status = WorkOrderStatus.Open.ToDatabaseValue()
                };
                _scannerRepository.AddWorkOrder(workOrder);
                await _scannerRepository.SaveChangesAsync(cancellationToken); // Genera ID
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
                if (targetStep.StepNumber > 1 && workOrder.Status == WorkOrderStatus.Open.ToDatabaseValue())
                {
                    workOrder.Status = WorkOrderStatus.InProgress.ToDatabaseValue();
                }
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


    public async Task<ServiceResponse<IReadOnlyList<ErrorCategoryResponse>>> GetErrorCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await _scannerRepository.GetActiveErrorCategoriesAsync(cancellationToken);
        var response = categories
            .Select(category => new ErrorCategoryResponse(category.Id, category.Name))
            .ToList();

        return ServiceResponse<IReadOnlyList<ErrorCategoryResponse>>.Ok(response);
    }

    public async Task<ServiceResponse<IReadOnlyList<ErrorCodeResponse>>> GetErrorCodesByCategoryAsync(uint categoryId, CancellationToken cancellationToken)
    {
        var codes = await _scannerRepository.GetActiveErrorCodesByCategoryAsync(categoryId, cancellationToken);
        var response = codes
            .Select(code => new ErrorCodeResponse(code.Id, code.Code, code.Description))
            .ToList();

        return ServiceResponse<IReadOnlyList<ErrorCodeResponse>>.Ok(response);
    }

    public async Task<ServiceResponse<ScrapResponse>> ScrapOrderAsync(ScrapOrderRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber))
        {
            return ServiceResponse<ScrapResponse>.Fail("Orden requerida.");
        }

        if (string.IsNullOrWhiteSpace(request.PartNumber))
        {
            return ServiceResponse<ScrapResponse>.Fail("Número de parte requerido.");
        }

        if (request.Quantity == 0)
        {
            return ServiceResponse<ScrapResponse>.Fail("Cantidad inválida.");
        }

        var normalizedWorkOrder = request.WorkOrderNumber.Trim();
        var normalizedPartNumber = request.PartNumber.Trim();

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

        var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(normalizedWorkOrder, cancellationToken);
        if (workOrder is null)
        {
            return ServiceResponse<ScrapResponse>.Fail("Orden no encontrada.");
        }

        if (!string.Equals(workOrder.Product?.PartNumber, normalizedPartNumber, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResponse<ScrapResponse>.Fail("El número de parte no corresponde a la orden.");
        }

        if (workOrder.Status.IsOneOf(WorkOrderStatus.Finished, WorkOrderStatus.Cancelled))
        {
            return ServiceResponse<ScrapResponse>.Fail("La orden no permite scrap.");
        }

        var wipItem = await _scannerRepository.GetWipItemByWorkOrderIdAsync(workOrder.Id, cancellationToken);
        if (wipItem is null)
        {
            return ServiceResponse<ScrapResponse>.Fail("WIP no encontrado.");
        }

        if (wipItem.Status.IsOneOf(WipItemStatus.Finished, WipItemStatus.Scrapped))
        {
            return ServiceResponse<ScrapResponse>.Fail("El WIP no permite scrap.");
        }

        var errorCode = await _scannerRepository.GetActiveErrorCodeByIdAsync(request.ErrorCodeId, cancellationToken);
        if (errorCode is null)
        {
            return ServiceResponse<ScrapResponse>.Fail("Código de error inválido.");
        }

        await _scannerRepository.ScrapOrderAsync(workOrder, wipItem, user, errorCode.Id, request.Quantity, request.Comments, cancellationToken);
        return ServiceResponse<ScrapResponse>.Ok(new ScrapResponse("Orden cancelada.", workOrder.Id, wipItem.Id));
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
}
