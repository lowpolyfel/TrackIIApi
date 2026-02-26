namespace Trackii.Api.Models;

public sealed class ScrapLog
{
    public uint Id { get; set; }
    public uint WipItemId { get; set; }
    public uint ErrorCodeId { get; set; }
    public uint RouteStepId { get; set; }
    public uint UserId { get; set; }
    public uint Qty { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }

    public WipItem? WipItem { get; set; }
    public ErrorCode? ErrorCode { get; set; }
    public RouteStep? RouteStep { get; set; }
    public User? User { get; set; }
}
