namespace Trackii.Api.Models;

public sealed class ErrorCode
{
    public uint Id { get; set; }
    public uint CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; }

    public ErrorCategory? ErrorCategory { get; set; }
}
