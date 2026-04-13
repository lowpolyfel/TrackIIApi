namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class Subfamily
{
    public uint Id { get; set; }
    public uint FamilyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Family? Family { get; set; }
    public SubfamilyActiveRoute? ActiveRouteMapping { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
