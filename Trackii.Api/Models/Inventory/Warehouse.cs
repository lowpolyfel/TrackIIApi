namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class Warehouse
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint? LocationId { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Location? Location { get; set; }
    public virtual ICollection<StockMovementLog> StockMovementLogs { get; set; } = new List<StockMovementLog>();
    public virtual ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    public virtual ICollection<InventorySnapshot> InventorySnapshots { get; set; } = new List<InventorySnapshot>();
}
