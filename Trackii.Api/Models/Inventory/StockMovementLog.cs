namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class StockMovementLog
{
    public uint Id { get; set; }
    public uint StockMovementItemId { get; set; }
    public uint ProductId { get; set; }
    public uint WarehouseId { get; set; }
    public int Qty { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual StockMovementItem? StockMovementItem { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
}
