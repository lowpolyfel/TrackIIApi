namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class WorkOrderItem
{
    public uint Id { get; set; }
    public uint WorkOrderId { get; set; }
    public uint UserId { get; set; }
    public string Status { get; set; } = "OPEN";
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
    public virtual User? User { get; set; }
    public virtual ICollection<WorkOrderLog> WorkOrderLogs { get; set; } = new List<WorkOrderLog>();
}
