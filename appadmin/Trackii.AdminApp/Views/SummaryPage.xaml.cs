using Trackii.AdminApp.ViewModels;

namespace Trackii.AdminApp.Views;

public partial class SummaryPage : ContentPage, IQueryAttributable
{
    private readonly SummaryViewModel _viewModel;

    public SummaryPage(SummaryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("summary", out var summary) && summary is string text)
        {
            _viewModel.ResultMessage = text;
        }
    }

    private async void OnRestartClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///ScanPage");
    }
}
