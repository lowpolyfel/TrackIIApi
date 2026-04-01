using System.Collections.ObjectModel;
using System.Windows.Input;
using Trackii.AdminApp.Models;
using Trackii.AdminApp.Services;

namespace Trackii.AdminApp.ViewModels;

public sealed class ScanViewModel : ViewModelBase
{
    private readonly ITrackiiAdminApiClient _apiClient;

    private string _workOrderNumber = string.Empty;
    private string _partNumber = string.Empty;
    private string _status = "Escanea o captura lote y producto.";
    private bool _isBusy;

    public ScanViewModel(ITrackiiAdminApiClient apiClient)
    {
        _apiClient = apiClient;
        ValidateCommand = new Command(async () => await ValidateAsync(), () => !IsBusy);
    }

    public string WorkOrderNumber
    {
        get => _workOrderNumber;
        set => SetProperty(ref _workOrderNumber, value);
    }

    public string PartNumber
    {
        get => _partNumber;
        set => SetProperty(ref _partNumber, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((Command)ValidateCommand).ChangeCanExecute();
            }
        }
    }

    public ICommand ValidateCommand { get; }

    private async Task ValidateAsync()
    {
        if (string.IsNullOrWhiteSpace(WorkOrderNumber) || string.IsNullOrWhiteSpace(PartNumber))
        {
            Status = "Debes capturar los dos códigos.";
            return;
        }

        try
        {
            IsBusy = true;
            var part = await _apiClient.GetPartInfoAsync(PartNumber);
            var context = await _apiClient.GetWorkOrderContextAsync(WorkOrderNumber, PartNumber);

            var draft = new WorkOrderDraft
            {
                WorkOrderNumber = WorkOrderNumber.Trim(),
                PartNumber = PartNumber.Trim(),
                PreviousQuantity = context?.PreviousQuantity ?? 0,
                NewQuantity = context?.PreviousQuantity ?? 0
            };

            await Shell.Current.GoToAsync(nameof(Views.RoutePage), true, new Dictionary<string, object>
            {
                [nameof(WorkOrderDraft)] = draft,
                ["partLookup"] = part,
                ["context"] = context
            });
        }
        catch (Exception ex)
        {
            Status = $"Error al consultar API: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
