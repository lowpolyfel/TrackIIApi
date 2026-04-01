using Trackii.AdminApp.Models;
using Trackii.AdminApp.ViewModels;

namespace Trackii.AdminApp.Views;

public partial class RoutePage : ContentPage, IQueryAttributable
{
    private readonly RouteViewModel _viewModel;

    public RoutePage(RouteViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        query.TryGetValue(nameof(WorkOrderDraft), out var draftObj);
        query.TryGetValue("partLookup", out var partObj);
        query.TryGetValue("context", out var contextObj);

        if (draftObj is WorkOrderDraft draft)
        {
            _viewModel.SetContext(draft, partObj as PartLookupResponse, contextObj as WorkOrderContextResponse);
        }
    }
}
