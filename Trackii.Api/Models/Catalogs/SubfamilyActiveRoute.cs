namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class SubfamilyActiveRoute
{
    public uint Id { get; set; }
    public uint SubfamilyId { get; set; }
    public uint RouteId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Subfamily? Subfamily { get; set; }
    public Route? Route { get; set; }
}
