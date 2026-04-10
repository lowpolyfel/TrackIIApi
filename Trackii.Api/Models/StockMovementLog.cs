namespace Trackii.Api.Models;

public sealed class StockMovementLog
{
    public uint Id { get; set; }
    public uint StockMovementItemId { get; set; }
    public uint ProductId { get; set; }
    public uint WarehouseId { get; set; }
    public uint Qty { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual StockMovementItem? StockMovementItem { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
}
