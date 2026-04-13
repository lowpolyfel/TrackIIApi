namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.

public class ErrorCode
{
    public uint Id { get; set; }
    public uint CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 🔥 Cambiamos Category por ErrorCategory
    public ErrorCategory? ErrorCategory { get; set; }
}
