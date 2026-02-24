namespace Trackii.Api.Contracts;

public sealed record WorkOrderContextResponse(
    bool IsNew,
    int PreviousQuantity,
    int CurrentStepNumber,
    string CurrentStepName,
    IReadOnlyList<NextRouteStepResponse> NextSteps);

public sealed record NextRouteStepResponse(
    uint StepId,
    int StepNumber,
    string StepName,
    uint LocationId,
    string LocationName);
