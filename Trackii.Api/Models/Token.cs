namespace Trackii.Api.Models;

public sealed class Token
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public uint? CreatedBy { get; set; }

    public User? CreatedByUser { get; set; }
}
