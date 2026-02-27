namespace Trackii.Api.Contracts;

public sealed record ReleaseWipItemResponse(
    uint WipItemId,
    string LotNumber,
    string Status,
    string Message);
