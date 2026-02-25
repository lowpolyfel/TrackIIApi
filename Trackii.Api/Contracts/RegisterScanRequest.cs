using System.Text.Json.Serialization;

namespace Trackii.Api.Contracts;

public sealed record RegisterScanRequest(
    [property: JsonPropertyName("WorkOrderNumber")]
    string WorkOrderNumber,
    [property: JsonPropertyName("PartNumber")]
    string PartNumber,
    [property: JsonPropertyName("Quantity")]
    int Quantity,
    [property: JsonPropertyName("UserId")]
    uint UserId,
    [property: JsonPropertyName("DeviceId")]
    uint DeviceId);
