namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class ReworkLog
{
    public uint Id { get; set; }
    public uint ReworkItemId { get; set; }
    public uint TargetRouteStepId { get; set; }
    public uint QtyReworked { get; set; }
    public ReworkReason Reason { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ReworkItem? ReworkItem { get; set; }
    public virtual RouteStep? TargetRouteStep { get; set; }
}
