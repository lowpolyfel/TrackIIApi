namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class WipStepExecution
{
    public uint Id { get; set; }
    public uint WipItemId { get; set; }
    public uint RouteStepId { get; set; }
    public uint DeviceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public uint QtyIn { get; set; }
    public uint QtyScrap { get; set; }

    public WipItem? WipItem { get; set; }
    public RouteStep? RouteStep { get; set; }
    public Device? Device { get; set; }
    public ICollection<ScrapItem> ScrapItems { get; set; } = new List<ScrapItem>();
}
