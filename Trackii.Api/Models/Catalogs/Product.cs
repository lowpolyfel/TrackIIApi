namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class Product
{
    public uint Id { get; set; }
    public uint SubfamilyId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Subfamily? Subfamily { get; set; }
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<StockMovementLog> StockMovementLogs { get; set; } = new List<StockMovementLog>();
    public ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    public ICollection<InventorySnapshot> InventorySnapshots { get; set; } = new List<InventorySnapshot>();
}
