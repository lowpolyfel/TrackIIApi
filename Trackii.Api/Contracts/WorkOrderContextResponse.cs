namespace Trackii.Api.Contracts;

public sealed record WorkOrderContextResponse(
    bool IsNew,
    string OrderStatus,
    string WipStatus,
    string? StatusUpdatedAt,
    int PreviousQuantity,
    int CurrentStepNumber,
    string CurrentStepName,
    string CurrentLocationName,
    string? RouteName,
    IReadOnlyList<NextRouteStepResponse> NextSteps,
    IReadOnlyList<TimelineStepResponse> Timeline);

public sealed record NextRouteStepResponse(
    uint StepId,
    int StepNumber,
    string StepName,
    uint LocationId,
    string LocationName);

public sealed record TimelineStepResponse(
    int StepOrder,
    string LocationName,
    string State,
    string Pieces,
    string Scrap,
    string? ErrorCode = null,
    string? Comments = null);