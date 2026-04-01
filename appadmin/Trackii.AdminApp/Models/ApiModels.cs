using System.Text.Json.Serialization;

namespace Trackii.AdminApp.Models;

public sealed class PartLookupResponse
{
    public bool Found { get; set; }
    public string? Message { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string? CurrentLocationName { get; set; }
    public string? NextLocationName { get; set; }
}

public sealed class WorkOrderContextResponse
{
    public bool IsNew { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string WipStatus { get; set; } = string.Empty;
    public int PreviousQuantity { get; set; }
    public int CurrentStepNumber { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public string CurrentLocationName { get; set; } = string.Empty;
    public string? RouteName { get; set; }
    public IReadOnlyList<NextRouteStepResponse> NextSteps { get; set; } = [];
    public IReadOnlyList<TimelineStepResponse> Timeline { get; set; } = [];
}

public sealed class NextRouteStepResponse
{
    public uint StepId { get; set; }
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public uint LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
}

public sealed class TimelineStepResponse
{
    public int StepOrder { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pieces { get; set; } = "0";
    public string Scrap { get; set; } = "0";
    public string? ErrorCode { get; set; }
    public string? Comments { get; set; }
}

public sealed class ErrorCategoryResponse
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class ErrorCodeResponse
{
    public uint Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class RegisterScanRequest
{
    [JsonPropertyName("WorkOrderNumber")]
    public string WorkOrderNumber { get; set; } = string.Empty;

    [JsonPropertyName("PartNumber")]
    public string PartNumber { get; set; } = string.Empty;

    [JsonPropertyName("Quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("ScrapQuantity")]
    public int ScrapQuantity { get; set; }

    [JsonPropertyName("ErrorCodeId")]
    public uint? ErrorCodeId { get; set; }

    [JsonPropertyName("Comments")]
    public string? Comments { get; set; }

    [JsonPropertyName("UserId")]
    public uint UserId { get; set; }

    [JsonPropertyName("DeviceId")]
    public uint DeviceId { get; set; }
}
