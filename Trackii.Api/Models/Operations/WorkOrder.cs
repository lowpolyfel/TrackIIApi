namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class WorkOrder
{
    public uint Id { get; set; }
    public string WoNumber { get; set; } = string.Empty;
    public uint ProductId { get; set; }
    public uint? RouteId { get; set; }
    public string Status { get; set; } = "OPEN";
    public uint QtyOrdered { get; set; } = 1;
    public uint QtyProduced { get; set; } = 0;
    public uint QtyScrapped { get; set; } = 0;
    public DateOnly? DueDate { get; set; }
    public byte Priority { get; set; } = 5;
    public uint? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Product? Product { get; set; }
    public Route? Route { get; set; }
    public User? CreatedByUser { get; set; }
    public WipItem? WipItem { get; set; }
    public ICollection<ReworkItem> ReworkItems { get; set; } = new List<ReworkItem>();
    public ICollection<WorkOrderItem> WorkOrderItems { get; set; } = new List<WorkOrderItem>();
    public ICollection<ProductionStats> ProductionStats { get; set; } = new List<ProductionStats>();
}
