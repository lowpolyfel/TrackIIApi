namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class Family
{
    public uint Id { get; set; }
    public uint AreaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Area? Area { get; set; }
    public ICollection<Subfamily> Subfamilies { get; set; } = new List<Subfamily>();
}
