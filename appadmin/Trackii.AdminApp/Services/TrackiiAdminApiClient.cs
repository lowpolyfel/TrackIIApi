using System.Net.Http.Json;
using System.Text.Json;
using Trackii.AdminApp.Models;

namespace Trackii.AdminApp.Services;

public sealed class TrackiiAdminApiClient(HttpClient httpClient) : ITrackiiAdminApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<PartLookupResponse?> GetPartInfoAsync(string partNumber, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<PartLookupResponse>($"api/scanner/part/{Uri.EscapeDataString(partNumber)}", JsonOptions, cancellationToken);

    public async Task<WorkOrderContextResponse?> GetWorkOrderContextAsync(string workOrderNumber, string partNumber, CancellationToken cancellationToken = default)
    {
        var route = $"api/scanner/work-orders/{Uri.EscapeDataString(workOrderNumber)}/context?deviceId={ApiConstants.DefaultDeviceId}&partNumber={Uri.EscapeDataString(partNumber)}";
        return await httpClient.GetFromJsonAsync<WorkOrderContextResponse>(route, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<ErrorCategoryResponse>> GetErrorCategoriesAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<IReadOnlyList<ErrorCategoryResponse>>("api/scanner/error-categories", JsonOptions, cancellationToken) ?? [];

    public async Task<IReadOnlyList<ErrorCodeResponse>> GetErrorCodesByCategoryAsync(uint categoryId, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<IReadOnlyList<ErrorCodeResponse>>($"api/scanner/error-categories/{categoryId}/codes", JsonOptions, cancellationToken) ?? [];

    public async Task<string> RegisterScanAsync(RegisterScanRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/scanner/register", request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
        return string.IsNullOrWhiteSpace(content) ? "Registro completado." : content;
    }
}
