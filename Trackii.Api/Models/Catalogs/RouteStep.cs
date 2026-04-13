namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class RouteStep
{
    public uint Id { get; set; }
    public uint RouteId { get; set; }
    public uint StepNumber { get; set; }
    public uint LocationId { get; set; }

    public Route? Route { get; set; }
    public Location? Location { get; set; }
    public ICollection<WipStepExecution> StepExecutions { get; set; } = new List<WipStepExecution>();
    public ICollection<ReworkLog> ReworkLogs { get; set; } = new List<ReworkLog>();
    public ICollection<ProductionStats> ProductionStats { get; set; } = new List<ProductionStats>();
}
