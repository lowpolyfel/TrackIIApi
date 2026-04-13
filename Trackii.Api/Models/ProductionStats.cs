namespace Trackii.Api.Models;

public class ProductionStats
{
    public uint LocationId { get; set; }
    public uint RouteStepId { get; set; }
    public uint WorkOrderId { get; set; }
    public DateOnly DateKey { get; set; }
    public uint TotalQtyIn { get; set; }
    public uint TotalQtyScrap { get; set; }
    public uint TotalQtyRework { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Location? Location { get; set; }
    public virtual RouteStep? RouteStep { get; set; }
    public virtual WorkOrder? WorkOrder { get; set; }
}
