namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class WipItem
{
    public uint Id { get; set; }
    public uint WorkOrderId { get; set; }
    public uint CurrentStepId { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public uint CurrentQty { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public WorkOrder? WorkOrder { get; set; }
    public RouteStep? CurrentStep { get; set; }
    public ICollection<WipStepExecution> StepExecutions { get; set; } = new List<WipStepExecution>();
}
