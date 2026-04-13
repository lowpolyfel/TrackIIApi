namespace Trackii.Api.Models;

public class Product
{
    public uint Id { get; set; }
    public uint SubfamilyId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public bool Active { get; set; }

    public Subfamily? Subfamily { get; set; }
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<StockMovementLog> StockMovementLogs { get; set; } = new List<StockMovementLog>();
    public ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();
    public ICollection<InventorySnapshot> InventorySnapshots { get; set; } = new List<InventorySnapshot>();
}
