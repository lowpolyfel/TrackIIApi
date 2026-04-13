namespace Trackii.Api.Models;

public class ScanEvent
{
    public uint Id { get; set; }
    public string? RawCode { get; set; }
    public string? RawWo { get; set; }
    public string? RawPart { get; set; }
    public uint DeviceId { get; set; }
    public DateTime ScannedAt { get; set; }
    public string Status { get; set; } = ScanEventStatus.SUCCESS.ToString();
    public string ErrorType { get; set; } = ScanEventErrorType.NONE.ToString();

    public Device? Device { get; set; }
}
