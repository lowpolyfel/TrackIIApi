namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class UnregisteredPart
{
    public uint Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; }
    public string Status { get; set; } = UnregisteredPartStatus.PENDING.ToString();
    public uint? ResolverUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public User? ResolverUser { get; set; }
}
