namespace Trackii.Api.Models;

public class User
{
    public uint Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public uint RoleId { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Role? Role { get; set; }
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<ReworkItem> ReworkItems { get; set; } = new List<ReworkItem>();
    public ICollection<WorkOrderItem> WorkOrderItems { get; set; } = new List<WorkOrderItem>();
    public ICollection<StockMovementItem> StockMovementItems { get; set; } = new List<StockMovementItem>();
    public ICollection<InventorySnapshot> InventorySnapshotsCreated { get; set; } = new List<InventorySnapshot>();
}
