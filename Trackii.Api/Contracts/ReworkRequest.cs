namespace Trackii.Api.Contracts;

public sealed record ReworkRequest(
    string WorkOrderNumber,
    string PartNumber,
    uint Quantity,
    uint LocationId,
    bool IsRelease,
    string? Reason,
    uint UserId,
    uint DeviceId
);
