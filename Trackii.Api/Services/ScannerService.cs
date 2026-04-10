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
                CreationDateTime = DateTime.Now,
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
        if (string.IsNullOrWhiteSpace(woNumber)) return ServiceResponse<WorkOrderContextResponse>.Fail("Número de orden requerido.");

        var normalizedWorkOrder = woNumber.Trim();
        var workOrder = await _scannerRepository.GetWorkOrderContextAsync(normalizedWorkOrder, cancellationToken);
        Product? product = workOrder?.Product;

        if (product is null && !string.IsNullOrWhiteSpace(partNumber))
        {
            product = await _scannerRepository.GetActiveProductWithSubfamilyAsync(partNumber.Trim(), cancellationToken);
        }

        var currentStepName = string.Empty;
        var currentLocationName = string.Empty;
        IReadOnlyList<NextRouteStepResponse> nextSteps = [];
        var timeline = new List<TimelineStepResponse>();

        // 🟢 Extraemos los Estatus y la Fecha de la orden
        string orderStatus = workOrder?.Status ?? "Open";
        string wipStatus = workOrder?.WipItem?.Status ?? "New";
        string? statusUpdatedAt = null;

        if (workOrder is null)
        {
            var routeName = product?.Subfamily?.ActiveRoute?.Name ?? "Sin ruta";
            if (product?.Subfamily?.ActiveRouteId is not null)
            {
                var pasosRuta = await _scannerRepository.GetRouteStepsByRouteIdAsync(product.Subfamily.ActiveRouteId.Value, cancellationToken);
                if (pasosRuta.Count > 0)
                {
                    var primerPaso = pasosRuta.FirstOrDefault(s => s.StepNumber == 1);
                    currentStepName = primerPaso?.Location?.Name ?? $"Paso {primerPaso?.StepNumber ?? 1}";
                    currentLocationName = primerPaso?.Location?.Name ?? $"Location {primerPaso?.LocationId ?? pasosRuta[0].LocationId}";
                    nextSteps = pasosRuta.Select(step => new NextRouteStepResponse(step.Id, (int)step.StepNumber, step.Location?.Name ?? $"Paso {step.StepNumber}", step.LocationId, step.Location?.Name ?? $"Location {step.LocationId}")).ToList();
                    timeline = pasosRuta.Select(step => new TimelineStepResponse((int)step.StepNumber, step.Location?.Name ?? $"Paso {step.StepNumber}", step.StepNumber == 1 ? "CURRENT" : "PENDING", "-", "-")).ToList();
                }
            }

            return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(
                IsNew: true, OrderStatus: orderStatus, WipStatus: wipStatus, StatusUpdatedAt: statusUpdatedAt,
                PreviousQuantity: 0, CurrentStepNumber: 1, CurrentStepName: currentStepName, CurrentLocationName: currentLocationName, RouteName: routeName, NextSteps: nextSteps, Timeline: timeline));
        }

        if (workOrder.Product?.Subfamily?.ActiveRouteId is null) return ServiceResponse<WorkOrderContextResponse>.Fail("La orden no tiene ruta activa configurada.");

        var routeSteps = await _scannerRepository.GetRouteStepsByRouteIdAsync(workOrder.Product.Subfamily.ActiveRouteId.Value, cancellationToken);
        if (routeSteps.Count == 0) return ServiceResponse<WorkOrderContextResponse>.Fail("La ruta no tiene pasos configurados.");

        var currentStepNumber = 1;
        currentStepName = routeSteps[0].Location?.Name ?? $"Paso {currentStepNumber}";
        currentLocationName = routeSteps[0].Location?.Name ?? $"Location {routeSteps[0].LocationId}";
        var previousQuantity = 0;

        nextSteps = routeSteps.Select(step => new NextRouteStepResponse(step.Id, (int)step.StepNumber, step.Location?.Name ?? $"Paso {step.StepNumber}", step.LocationId, step.Location?.Name ?? $"Location {step.LocationId}")).ToList();

        if (workOrder.WipItem is not null)
        {
            var currentStep = routeSteps.FirstOrDefault(step => step.Id == workOrder.WipItem.CurrentStepId);
            if (currentStep is not null)
            {
                currentStepNumber = (int)currentStep.StepNumber;
                currentStepName = currentStep.Location?.Name ?? $"Paso {currentStep.StepNumber}";
                currentLocationName = currentStep.Location?.Name ?? $"Location {currentStep.LocationId}";
                nextSteps = routeSteps.Where(step => step.StepNumber > currentStep.StepNumber).Select(step => new NextRouteStepResponse(step.Id, (int)step.StepNumber, step.Location?.Name ?? $"Paso {step.StepNumber}", step.LocationId, step.Location?.Name ?? $"Location {step.LocationId}")).ToList();
            }

            var latestExecution = await _scannerRepository.GetLatestExecutionByWipItemIdAsync(workOrder.WipItem.Id, cancellationToken);
            if (latestExecution is not null)
            {
                previousQuantity = (int)latestExecution.QtyIn;
                statusUpdatedAt = latestExecution.CreatedAt.ToString("dd/MM/yyyy HH:mm");
            }

            // 🔴 Buscar detalles de Scrap si la orden está cancelada
            bool isCancelled = orderStatus == "Cancelled" || wipStatus == "Scrapped";
            ScrapLog? scrapLog = null;
            if (isCancelled)
            {
                scrapLog = await _scannerRepository.GetScrapLogByWipItemIdAsync(workOrder.WipItem.Id, cancellationToken);
            }

            int previousStepPieces = -1; // Usado para calcular diferencia

            var sortedSteps = routeSteps.OrderBy(s => s.StepNumber).ToList();
            foreach (var step in sortedSteps)
            {
                // ✂️ Si está cancelada y ya pasamos el paso donde ocurrió el problema, cortamos.
                if (isCancelled && step.StepNumber > currentStepNumber)
                    break;

                if (step.StepNumber < currentStepNumber)
                {
                    var execution = workOrder.WipItem.StepExecutions?.FirstOrDefault(e => e.RouteStepId == step.Id);
                    int currentPieces = execution != null ? (int)execution.QtyIn : 0;

                    // 🧮 Calcular Scrap: Comparamos piezas de este paso vs. el paso que le siguió
                    int calculatedScrap = 0;
                    if (previousStepPieces != -1 && previousStepPieces > currentPieces)
                    {
                        calculatedScrap = previousStepPieces - currentPieces;
                    }

                    timeline.Add(new TimelineStepResponse(
                        StepOrder: (int)step.StepNumber,
                        LocationName: step.Location?.Name ?? $"Paso {step.StepNumber}",
                        State: "DONE",
                        Pieces: currentPieces > 0 ? currentPieces.ToString() : "-",
                        Scrap: calculatedScrap > 0 ? calculatedScrap.ToString() : (execution?.QtyScrap > 0 ? execution.QtyScrap.ToString() : "-")
                    ));

                    // Guardamos para el ciclo de la siguiente estación
                    if (currentPieces > 0)
                    {
                        previousStepPieces = currentPieces;
                    }
                }
                else if (step.StepNumber == currentStepNumber)
                {
                    int currentPieces = previousQuantity;

                    int calculatedScrap = 0;
                    if (previousStepPieces != -1 && previousStepPieces > currentPieces)
                    {
                        calculatedScrap = previousStepPieces - currentPieces;
                    }

                    // Asignar el estado visual si fue aquí donde murió la orden
                    string state = isCancelled ? "CANCELLED" : "CURRENT";

                    // 🟢 Creamos el texto formateado: "NombreCategoria | CODIGO - Descripcion"
                    // 🟢 Creamos el texto formateado: "NombreCategoria | CODIGO - Descripcion"
                    string? errorDisplay = null;
                    if (scrapLog?.ErrorCode != null)
                    {
                        string categoryName = scrapLog.ErrorCode.ErrorCategory?.Name ?? "Sin Categoría"; // 🔥 Usamos ErrorCategory
                        string codeName = scrapLog.ErrorCode.Code;
                        string desc = scrapLog.ErrorCode.Description;
                        errorDisplay = $"{categoryName} | {codeName} - {desc}";
                    }
                    timeline.Add(new TimelineStepResponse(
                        StepOrder: (int)step.StepNumber,
                        LocationName: step.Location?.Name ?? $"Paso {step.StepNumber}",
                        State: state,
                        Pieces: currentPieces > 0 ? currentPieces.ToString() : "-",
                        Scrap: calculatedScrap > 0 ? calculatedScrap.ToString() : "-",
                        ErrorCode: errorDisplay,
                        Comments: scrapLog?.Comments
                    ));
                }
                else
                {
                    timeline.Add(new TimelineStepResponse(
                        StepOrder: (int)step.StepNumber,
                        LocationName: step.Location?.Name ?? $"Paso {step.StepNumber}",
                        State: "PENDING",
                        Pieces: "-",
                        Scrap: "-"
                    ));
                }
            }
        }

        return ServiceResponse<WorkOrderContextResponse>.Ok(new WorkOrderContextResponse(
            IsNew: workOrder.WipItem is null, OrderStatus: orderStatus, WipStatus: wipStatus, StatusUpdatedAt: statusUpdatedAt,
            PreviousQuantity: previousQuantity, CurrentStepNumber: currentStepNumber, CurrentStepName: currentStepName,
            CurrentLocationName: currentLocationName, RouteName: workOrder.Product?.Subfamily?.ActiveRoute?.Name, NextSteps: nextSteps, Timeline: timeline));
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
                    CreationDateTime = DateTime.Now,
                    Active = true
                });
                _scannerRepository.AddScanEvent(new ScanEvent
                {
                    ScanType = ScanType.Error.ToDatabaseValue(),
                    Ts = DateTime.Now
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
                bool isInvalidStatus = false;
                string errorMessage = string.Empty;

                if (workOrder.Status.IsOneOf(WorkOrderStatus.Cancelled, WorkOrderStatus.Finished))
                {
                    isInvalidStatus = true;
                    errorMessage = "La orden no permite avanzar porque está terminada o cancelada.";
                }
                else if (workOrder.ProductId != product.Id)
                {
                    return ServiceResponse<RegisterScanResponse>.Fail("El número de parte no corresponde a la orden.");
                }
                else if (wipItem is not null && wipItem.Status.IsOneOf(WipItemStatus.Finished, WipItemStatus.Scrapped, WipItemStatus.Hold))
                {
                    isInvalidStatus = true;
                    errorMessage = $"El producto no puede avanzar porque se encuentra en: {wipItem.Status}.";
                }

                // Si la orden o el WIP están en un estado bloqueado
                if (isInvalidStatus)
                {
                    // Guardamos el evento de tipo ERROR en scan_event si el wipItem ya existe
                    if (wipItem is not null)
                    {
                        _scannerRepository.AddScanEvent(new ScanEvent
                        {
                            WipItemId = wipItem.Id,
                            RouteStepId = wipItem.CurrentStepId,
                            ScanType = ScanType.Error.ToDatabaseValue(),
                            Ts = DateTime.Now
                        });

                        await _scannerRepository.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken); // Hacemos commit del error para que persista
                    }

                    return ServiceResponse<RegisterScanResponse>.Fail(errorMessage);
                }
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
                    CreatedAt = DateTime.Now,
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

            // 1. Registramos la ejecución con la cantidad buena y el scrap
            _scannerRepository.AddWipStepExecution(new WipStepExecution
            {
                WipItem = wipItem!,
                RouteStepId = targetStep.Id,
                UserId = user.Id,
                DeviceId = device.Id,
                LocationId = device.LocationId,
                CreatedAt = DateTime.Now,
                QtyIn = (uint)request.Quantity,
                QtyScrap = (uint)request.ScrapQuantity // AHORA TOMA EL VALOR DE ANDROID
            });

            // 2. Si hubo scrap, se registra bajo el patrón Item & Log
            if (request.ScrapQuantity > 0 && request.ErrorCodeId.HasValue)
            {
                var latestExecutionForScrap = await _scannerRepository.GetLatestExecutionByWipItemIdAsync(wipItem!.Id, cancellationToken);
                if (latestExecutionForScrap is not null)
                {
                    var scrapItem = new ScrapItem
                    {
                        WipStepExecutionId = latestExecutionForScrap.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _scannerRepository.AddScrapItem(scrapItem);
                    await _scannerRepository.SaveChangesAsync(cancellationToken);

                    _scannerRepository.AddScrapLog(new ScrapLog
                    {
                        ScrapItemId = scrapItem.Id,
                        ErrorCodeId = request.ErrorCodeId.Value,
                        QtyScrapped = (uint)request.ScrapQuantity,
                        Comments = request.Comments,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            _scannerRepository.AddScanEvent(new ScanEvent
            {
                WipItem = wipItem,
                RouteStepId = targetStep.Id,
                ScanType = ScanType.Entry.ToDatabaseValue(),
                Ts = DateTime.Now
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
                    Ts = DateTime.Now
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


    public async Task<ServiceResponse<ScrapResponse>> RegisterPartialScrapAsync(ScrapOrderRequest request, CancellationToken cancellationToken)
    {
        var normalizedWorkOrder = request.WorkOrderNumber.Trim();

        var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(normalizedWorkOrder, cancellationToken);
        if (workOrder is null) return ServiceResponse<ScrapResponse>.Fail("Orden no encontrada.");

        var wipItem = await _scannerRepository.GetWipItemByWorkOrderIdAsync(workOrder.Id, cancellationToken);
        if (wipItem is null) return ServiceResponse<ScrapResponse>.Fail("WIP no encontrado.");

        var user = await _scannerRepository.GetActiveUserByIdAsync(request.UserId, cancellationToken);
        if (user is null) return ServiceResponse<ScrapResponse>.Fail("Usuario inválido.", ServiceErrorType.Unauthorized);

        var errorCode = await _scannerRepository.GetActiveErrorCodeByIdAsync(request.ErrorCodeId, cancellationToken);
        if (errorCode is null) return ServiceResponse<ScrapResponse>.Fail("Código de error inválido.");

        var latestExecution = await _scannerRepository.GetLatestExecutionByWipItemIdAsync(wipItem.Id, cancellationToken);
        if (latestExecution is null)
        {
            return ServiceResponse<ScrapResponse>.Fail("No hay ejecución disponible para registrar scrap.");
        }

        var scrapItem = new ScrapItem
        {
            WipStepExecutionId = latestExecution.Id,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _scannerRepository.AddScrapItem(scrapItem);
        await _scannerRepository.SaveChangesAsync(cancellationToken);

        _scannerRepository.AddScrapLog(new ScrapLog
        {
            ScrapItemId = scrapItem.Id,
            ErrorCodeId = errorCode.Id,
            QtyScrapped = request.Quantity,
            Comments = request.Comments,
            CreatedAt = DateTime.Now
        });

        await _scannerRepository.SaveChangesAsync(cancellationToken);

        return ServiceResponse<ScrapResponse>.Ok(new ScrapResponse("Scrap parcial registrado.", workOrder.Id, wipItem.Id));
    }


    public async Task<ServiceResponse<WipItemValidationResponse>> ValidateReworkAsync(string noLote, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(noLote))
        {
            return ServiceResponse<WipItemValidationResponse>.Fail("Número de lote requerido.");
        }

        var normalizedLotNumber = noLote.Trim();
        var wipItem = await _scannerRepository.GetWipItemByLotNumberAsync(normalizedLotNumber, cancellationToken);
        if (wipItem is null || wipItem.WorkOrder is null)
        {
            return ServiceResponse<WipItemValidationResponse>.Ok(new WipItemValidationResponse(
                false,
                null,
                null,
                normalizedLotNumber,
                null,
                null,
                null,
                "Esta orden aún no empieza."));
        }

        return ServiceResponse<WipItemValidationResponse>.Ok(new WipItemValidationResponse(
            true,
            wipItem.Id,
            wipItem.WorkOrderId,
            wipItem.WorkOrder.WoNumber,
            wipItem.CurrentStepId,
            wipItem.RouteId,
            wipItem.Status,
            null));
    }

    public async Task<ServiceResponse<ReleaseWipItemResponse>> ReleaseWipItemAsync(string noLote, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(noLote))
        {
            return ServiceResponse<ReleaseWipItemResponse>.Fail("Número de lote requerido.");
        }

        var normalizedLotNumber = noLote.Trim();
        var wipItem = await _scannerRepository.GetWipItemByLotNumberAsync(normalizedLotNumber, cancellationToken);
        if (wipItem is null || wipItem.WorkOrder is null)
        {
            return ServiceResponse<ReleaseWipItemResponse>.Fail("Esta orden aún no empieza.", ServiceErrorType.NotFound);
        }

        wipItem.Status = WipItemStatus.Active.ToDatabaseValue();
        await _scannerRepository.SaveChangesAsync(cancellationToken);

        return ServiceResponse<ReleaseWipItemResponse>.Ok(new ReleaseWipItemResponse(
            wipItem.Id,
            wipItem.WorkOrder.WoNumber,
            wipItem.Status,
            "Lote liberado exitosamente."));
    }

    public async Task<ServiceResponse<ReworkResponse>> ProcessReworkAsync(ReworkRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.WorkOrderNumber) || string.IsNullOrWhiteSpace(request.PartNumber))
        {
            return ServiceResponse<ReworkResponse>.Fail("La orden y el número de parte son requeridos.");
        }

        if (request.Quantity == 0)
        {
            return ServiceResponse<ReworkResponse>.Fail("Cantidad inválida.");
        }

        var workOrder = await _scannerRepository.GetWorkOrderContextAsync(request.WorkOrderNumber.Trim(), cancellationToken);
        if (workOrder is null || workOrder.WipItem is null)
        {
            return ServiceResponse<ReworkResponse>.Fail("La orden no existe o no ha sido iniciada.");
        }

        if (!string.Equals(workOrder.Product?.PartNumber, request.PartNumber.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResponse<ReworkResponse>.Fail("El número de parte no corresponde a la orden.");
        }

        if (workOrder.Status == WorkOrderStatus.Finished.ToDatabaseValue() ||
            workOrder.Status == WorkOrderStatus.Cancelled.ToDatabaseValue())
        {
            return ServiceResponse<ReworkResponse>.Fail("No se puede retrabajar una orden terminada o cancelada.");
        }

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

        var location = await _scannerRepository.GetLocationByIdAsync(request.LocationId, cancellationToken);
        if (location is null)
        {
            return ServiceResponse<ReworkResponse>.Fail("Localidad inválida.");
        }

        await using var transaction = await _scannerRepository.BeginTransactionAsync(cancellationToken);

        try
        {
            workOrder.WipItem.Status = request.IsRelease
                ? WipItemStatus.Active.ToDatabaseValue()
                : WipItemStatus.Hold.ToDatabaseValue();

            await _scannerRepository.SaveChangesAsync(cancellationToken);

            var reworkItem = new ReworkItem
            {
                WorkOrderId = workOrder.Id,
                UserId = request.UserId,
                Status = request.IsRelease ? "RELEASED" : "HOLD",
                Comment = request.Reason,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _scannerRepository.AddReworkItem(reworkItem);
            await _scannerRepository.SaveChangesAsync(cancellationToken);

            var reworkLog = new ReworkLog
            {
                ReworkItemId = reworkItem.Id,
                TargetRouteStepId = workOrder.WipItem.CurrentStepId,
                QtyReworked = request.Quantity,
                Reason = ReworkReason.Other,
                Comments = request.Reason,
                CreatedAt = DateTime.Now
            };

            await _scannerRepository.AddReworkLogAsync(reworkLog, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return ServiceResponse<ReworkResponse>.Ok(new ReworkResponse(true, "Retrabajo registrado exitosamente."));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ServiceResponse<ReworkResponse>.Fail("Error interno al procesar el retrabajo.");
        }
    }


    public async Task<ServiceResponse<bool>> ValidateAdvanceLocationAsync(string noLote, string partNumber, uint deviceId, CancellationToken cancellationToken)
    {
        var device = await _scannerRepository.GetActiveDeviceByIdAsync(deviceId, cancellationToken);
        if (device is null) return ServiceResponse<bool>.Fail("Dispositivo inválido.");

        var product = await _scannerRepository.GetActiveProductWithSubfamilyAsync(partNumber.Trim(), cancellationToken);
        if (product?.Subfamily?.ActiveRouteId is null) return ServiceResponse<bool>.Fail("El producto no tiene ruta configurada.");

        var steps = await _scannerRepository.GetRouteStepsByRouteIdAsync(product.Subfamily.ActiveRouteId.Value, cancellationToken);
        if (steps.Count == 0) return ServiceResponse<bool>.Fail("La ruta no tiene pasos.");

        var workOrder = await _scannerRepository.GetWorkOrderForRegisterAsync(noLote.Trim(), cancellationToken);
        var wipItem = workOrder is null ? null : await _scannerRepository.GetWipItemByWorkOrderIdAsync(workOrder.Id, cancellationToken);

        RouteStep targetStep;
        if (wipItem is null)
        {
            targetStep = steps.First();
        }
        else
        {
            var currentStep = steps.FirstOrDefault(s => s.Id == wipItem.CurrentStepId);
            if (currentStep is null) return ServiceResponse<bool>.Fail("Paso actual inválido.");

            targetStep = steps.FirstOrDefault(s => s.StepNumber == currentStep.StepNumber + 1);
            if (targetStep is null) return ServiceResponse<bool>.Fail("La orden ya está en su último paso.");
        }

        if (targetStep.LocationId != device.LocationId)
        {
            return ServiceResponse<bool>.Fail($"La tableta debe estar en: {targetStep.Location?.Name}");
        }

        return ServiceResponse<bool>.Ok(true);
    }

    public async Task<ServiceResponse<int>> GetDailyOrdersCountAsync(int locationId, CancellationToken cancellationToken)
    {
        if (locationId <= 0)
        {
            return ServiceResponse<int>.Fail("Localidad inválida.");
        }

        // Hacemos el cast a uint porque el Repositorio y la BD manejan el LocationId como uint
        var count = await _scannerRepository.GetDailyOrdersCountAsync((uint)locationId, cancellationToken);

        return ServiceResponse<int>.Ok(count);
    }
}