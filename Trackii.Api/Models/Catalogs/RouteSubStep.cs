namespace Trackii.Api.Models;

public class RouteSubStep
{
    public uint Id { get; set; }
    public uint RouteStepId { get; set; }
    public uint SubStepNumber { get; set; }
    public uint LocationId { get; set; }

    public RouteStep? RouteStep { get; set; }
    public Location? Location { get; set; }
}
