namespace Trackii.Api.Models;

public sealed class InventoryBalance
{
    public uint ProductId { get; set; }
    public uint WarehouseId { get; set; }
    public uint QtyOnHand { get; set; }
    public uint QtyReserved { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Product? Product { get; set; }
    public virtual Warehouse? Warehouse { get; set; }
}
