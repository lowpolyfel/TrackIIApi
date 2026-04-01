namespace Trackii.AdminApp.Models;

public sealed class WorkOrderDraft
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public int NewQuantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int CalculatedScrap => Math.Max(0, PreviousQuantity - NewQuantity);
    public uint? ErrorCategoryId { get; set; }
    public uint? ErrorCodeId { get; set; }
    public string? Comments { get; set; }
}
