namespace Trackii.Api.Contracts;

public sealed record WipItemValidationResponse(
    bool Exists,
    uint? WipItemId,
    uint? WorkOrderId,
    string? LotNumber,
    uint? CurrentStepId,
    uint? RouteId,
    string? Status,
    string? Message);
