namespace Trackii.Api.Models;

public class Area
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Family> Families { get; set; } = new List<Family>();
}
