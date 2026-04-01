namespace Trackii.AdminApp.ViewModels;

public sealed class SummaryViewModel : ViewModelBase
{
    private string _resultMessage = "Registro finalizado.";

    public string ResultMessage
    {
        get => _resultMessage;
        set => SetProperty(ref _resultMessage, value);
    }
}
