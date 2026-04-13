namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class SystemToken
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; } = 0;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public uint? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? CreatedByUser { get; set; }
}
