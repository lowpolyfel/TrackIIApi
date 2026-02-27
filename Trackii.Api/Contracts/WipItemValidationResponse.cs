namespace Trackii.Api.Contracts;

public sealed record WipItemValidationResponse(
    uint WipItemId,
    uint WorkOrderId,
    string LotNumber,
    uint CurrentStepId,
    uint RouteId,
    string Status);
