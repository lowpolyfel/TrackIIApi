namespace Trackii.Api.Models;

public enum ScanType
{
    Entry,
    Exit,
    Error,
    Manual
}

public enum WipItemStatus
{
    Active,
    Hold,
    Finished,
    Scrapped
}

public enum WorkOrderStatus
{
    Open,
    InProgress,
    Finished,
    Cancelled
}

public static class DomainEnumMapper
{
    public static string ToDatabaseValue(this ScanType scanType) => scanType switch
    {
        ScanType.Entry => "ENTRY",
        ScanType.Exit => "EXIT",
        ScanType.Error => "ERROR",
        ScanType.Manual => "MANUAL",
        _ => throw new ArgumentOutOfRangeException(nameof(scanType), scanType, null)
    };

    public static string ToDatabaseValue(this WipItemStatus status) => status switch
    {
        WipItemStatus.Active => "ACTIVE",
        WipItemStatus.Hold => "HOLD",
        WipItemStatus.Finished => "FINISHED",
        WipItemStatus.Scrapped => "SCRAPPED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string ToDatabaseValue(this WorkOrderStatus status) => status switch
    {
        WorkOrderStatus.Open => "OPEN",
        WorkOrderStatus.InProgress => "IN_PROGRESS",
        WorkOrderStatus.Finished => "FINISHED",
        WorkOrderStatus.Cancelled => "CANCELLED",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static bool IsOneOf(this string? value, params WipItemStatus[] statuses) =>
        value is not null && statuses.Any(status => value.Equals(status.ToDatabaseValue(), StringComparison.OrdinalIgnoreCase));

    public static bool IsOneOf(this string? value, params WorkOrderStatus[] statuses) =>
        value is not null && statuses.Any(status => value.Equals(status.ToDatabaseValue(), StringComparison.OrdinalIgnoreCase));
}
