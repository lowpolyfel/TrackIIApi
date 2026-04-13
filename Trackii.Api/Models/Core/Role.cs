namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class Role
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
}
