using Trackii.AdminApp.ViewModels;

namespace Trackii.AdminApp.Views;

public partial class ScanPage : ContentPage
{
    public ScanPage(ScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
