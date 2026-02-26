namespace Trackii.Api.Contracts;

public sealed record ErrorCodeResponse(
    uint Id,
    string Code,
    string Description);
