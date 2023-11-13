using Heating_Control_UI.ViewModels.Pages;

namespace Heating_Control_UI.Views.Pages;
public partial class HeatingControlView : PageControl
{
    public HeatingControlView(HeatingControlViewModel heatingControlViewModel)
    {
        InitializeComponent();
        DataContext = heatingControlViewModel;
        this.NavigatedTo += HeatingControlView_NavigatedTo;
    }

    public HeatingControlView()
    {
        InitializeComponent();
        DataContext = new HeatingControlViewModel();
    }

    private void HeatingControlView_NavigatedTo(object? sender, System.EventArgs e)
    {
        if (this.DataContext is HeatingControlViewModel viewModel)
            viewModel.NavigatedTo();
    }
}