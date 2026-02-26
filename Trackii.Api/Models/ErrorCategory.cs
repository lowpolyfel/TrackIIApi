namespace Trackii.Api.Models;

public sealed class ErrorCategory
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }

    public ICollection<ErrorCode> ErrorCodes { get; set; } = new List<ErrorCode>();
}
