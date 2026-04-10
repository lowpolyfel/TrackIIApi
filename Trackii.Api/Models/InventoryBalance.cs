namespace Trackii.Api.Models;

public sealed class InventoryBalance
{
    public uint ProductId { get; set; }
    public uint WarehouseId { get; set; }
    public int QtyOnHand { get; set; }
    public int QtyReserved { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Product? Product { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
}
