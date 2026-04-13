namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class WorkOrderLog
{
    public uint Id { get; set; }
    public uint WorkOrderItemId { get; set; }
    public WorkOrderEventType EventType { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Comment { get; set; }
    public DateTime ModifiedAt { get; set; }

    public virtual WorkOrderItem? WorkOrderItem { get; set; }
}
