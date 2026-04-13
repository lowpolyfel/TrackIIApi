namespace Trackii.Api.Models;

public class StockMovementItem
{
    public uint Id { get; set; }
    public StockMovementType MovementType { get; set; }
    public StockReferenceType ReferenceType { get; set; }
    public uint ReferenceId { get; set; }
    public uint UserId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<StockMovementLog> StockMovementLogs { get; set; } = new List<StockMovementLog>();
}
