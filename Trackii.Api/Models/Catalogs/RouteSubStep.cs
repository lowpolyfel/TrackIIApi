namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class RouteSubStep
{
    public uint Id { get; set; }
    public uint RouteStepId { get; set; }
    public uint SubStepNumber { get; set; }
    public uint LocationId { get; set; }

    public RouteStep? RouteStep { get; set; }
    public Location? Location { get; set; }
}
