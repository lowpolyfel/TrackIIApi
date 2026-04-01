using System.Collections.ObjectModel;
using System.Windows.Input;
using Trackii.AdminApp.Models;
using Trackii.AdminApp.Services;

namespace Trackii.AdminApp.ViewModels;

public sealed class RouteViewModel : ViewModelBase
{
    private readonly ITrackiiAdminApiClient _apiClient;
    private WorkOrderDraft _draft = new();

    private string _header = string.Empty;
    private int _newQuantity;
    private string _comments = string.Empty;
    private ErrorCategoryResponse? _selectedCategory;
    private ErrorCodeResponse? _selectedCode;

    public RouteViewModel(ITrackiiAdminApiClient apiClient)
    {
        _apiClient = apiClient;
        Categories = [];
        ErrorCodes = [];
        Timeline = [];
        LoadDataCommand = new Command(async () => await LoadDataAsync());
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public ObservableCollection<ErrorCategoryResponse> Categories { get; }
    public ObservableCollection<ErrorCodeResponse> ErrorCodes { get; }
    public ObservableCollection<TimelineStepResponse> Timeline { get; }

    public ICommand LoadDataCommand { get; }
    public ICommand SaveCommand { get; }

    public string Header { get => _header; set => SetProperty(ref _header, value); }

    public int NewQuantity
    {
        get => _newQuantity;
        set
        {
            if (SetProperty(ref _newQuantity, value))
            {
                OnPropertyChanged(nameof(CalculatedScrap));
            }
        }
    }

    public int CalculatedScrap => Math.Max(0, _draft.PreviousQuantity - NewQuantity);

    public string Comments { get => _comments; set => SetProperty(ref _comments, value); }

    public ErrorCategoryResponse? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value) && value is not null)
            {
                _ = LoadErrorCodesAsync(value.Id);
            }
        }
    }

    public ErrorCodeResponse? SelectedCode { get => _selectedCode; set => SetProperty(ref _selectedCode, value); }

    public void SetContext(WorkOrderDraft draft, PartLookupResponse? part, WorkOrderContextResponse? context)
    {
        _draft = draft;
        Header = $"Lote {_draft.WorkOrderNumber} / Parte {_draft.PartNumber}";
        NewQuantity = _draft.NewQuantity;

        Timeline.Clear();
        if (context?.Timeline is not null)
        {
            foreach (var item in context.Timeline) Timeline.Add(item);
        }

        if (part?.Found == false)
        {
            Header += " (No encontrado, registro manual habilitado)";
        }
    }

    private async Task LoadDataAsync()
    {
        Categories.Clear();
        foreach (var category in await _apiClient.GetErrorCategoriesAsync())
        {
            Categories.Add(category);
        }
    }

    private async Task LoadErrorCodesAsync(uint categoryId)
    {
        ErrorCodes.Clear();
        foreach (var code in await _apiClient.GetErrorCodesByCategoryAsync(categoryId))
        {
            ErrorCodes.Add(code);
        }
    }

    private async Task SaveAsync()
    {
        _draft.NewQuantity = NewQuantity;
        _draft.ErrorCategoryId = SelectedCategory?.Id;
        _draft.ErrorCodeId = SelectedCode?.Id;
        _draft.Comments = Comments;

        var request = new RegisterScanRequest
        {
            WorkOrderNumber = _draft.WorkOrderNumber,
            PartNumber = _draft.PartNumber,
            Quantity = _draft.NewQuantity,
            ScrapQuantity = _draft.CalculatedScrap,
            ErrorCodeId = _draft.ErrorCodeId,
            Comments = _draft.Comments,
            UserId = ApiConstants.DefaultUserId,
            DeviceId = ApiConstants.DefaultDeviceId
        };

        var message = await _apiClient.RegisterScanAsync(request);

        await Shell.Current.GoToAsync(nameof(Views.SummaryPage), true, new Dictionary<string, object>
        {
            ["summary"] = $"{message}\nPiezas: {_draft.NewQuantity}\nScrap: {_draft.CalculatedScrap}"
        });
    }
}
