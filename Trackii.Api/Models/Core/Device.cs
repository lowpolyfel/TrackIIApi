namespace Trackii.Api.Models;

// NOTE: File rewritten in domain folder; namespace kept as Trackii.Api.Models intentionally.
public class Device
{
    public uint Id { get; set; }
    public string DeviceUid { get; set; } = string.Empty;
    public uint LocationId { get; set; }
    public uint? UserId { get; set; }
    public string? Name { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Location? Location { get; set; }
    public User? User { get; set; }
}
