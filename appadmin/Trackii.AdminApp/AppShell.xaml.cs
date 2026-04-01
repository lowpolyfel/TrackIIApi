namespace Trackii.AdminApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Views.RoutePage), typeof(Views.RoutePage));
        Routing.RegisterRoute(nameof(Views.SummaryPage), typeof(Views.SummaryPage));
    }
}
