namespace Trackii.Api.Contracts;

public sealed record ScrapOrderRequest(
    string WorkOrderNumber,
    string PartNumber,
    uint Quantity,
    uint ErrorCodeId,
    string? Comments,
    uint UserId,
    uint DeviceId);
