using System.Text.Json.Serialization;

namespace Trackii.Api.Contracts;

using System.Text.Json.Serialization;

namespace Trackii.Api.Contracts;

public sealed record RegisterScanRequest(
    [property: JsonPropertyName("WorkOrderNumber")]
    string WorkOrderNumber,

    [property: JsonPropertyName("PartNumber")]
    string PartNumber,

    [property: JsonPropertyName("Quantity")]
    int Quantity,

    [property: JsonPropertyName("ScrapQuantity")]
    int ScrapQuantity,

    [property: JsonPropertyName("ErrorCodeId")]
    uint? ErrorCodeId,

    [property: JsonPropertyName("Comments")]
    string? Comments,

    [property: JsonPropertyName("UserId")]
    uint UserId,

    [property: JsonPropertyName("DeviceId")]
    uint DeviceId
);