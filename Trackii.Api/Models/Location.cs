namespace Trackii.Api.Models;

public class Location
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }

    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public ICollection<ProductionStats> ProductionStats { get; set; } = new List<ProductionStats>();
}
