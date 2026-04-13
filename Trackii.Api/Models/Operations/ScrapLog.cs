namespace Trackii.Api.Models;

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
