namespace Trackii.Api.Models;

public class ReworkItem
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
    public virtual ICollection<ReworkLog> ReworkLogs { get; set; } = new List<ReworkLog>();
}
