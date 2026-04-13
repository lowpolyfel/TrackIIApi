namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class ScrapLog
{
    public uint Id { get; set; }
    public uint ScrapItemId { get; set; }
    public uint ErrorCodeId { get; set; }
    public uint QtyScrapped { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ScrapItem? ScrapItem { get; set; }
    public virtual ErrorCode? ErrorCode { get; set; }
}
