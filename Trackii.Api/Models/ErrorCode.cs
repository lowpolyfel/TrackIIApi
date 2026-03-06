namespace Trackii.Api.Models;

public class ErrorCode
{
    public uint Id { get; set; }
    public uint CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; }

    // 🔥 Cambiamos Category por ErrorCategory
    public ErrorCategory? ErrorCategory { get; set; }
}