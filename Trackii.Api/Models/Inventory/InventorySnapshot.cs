namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class InventorySnapshot
{
    public uint Id { get; set; }
    public uint ProductId { get; set; }
    public uint WarehouseId { get; set; }
    public int Qty { get; set; }
    public DateTime SnapshotAt { get; set; }
    public uint CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Product? Product { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
    public virtual User? CreatedByUser { get; set; }
}
