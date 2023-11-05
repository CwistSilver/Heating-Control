using Heating_Control_UI.Utilities;
using Heating_Control_UI.ViewModels;

namespace Heating_Control_UI;

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
        DataContext = new HeatingControlViewModel(null);
    }

    private void HeatingControlView_NavigatedTo(object? sender, System.EventArgs e)
    {
        if (this.DataContext is HeatingControlViewModel viewModel)
            viewModel.NavigatedTo();
    }

    //private void UserControl_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    //{
    //    if (this.DataContext is HeatingControlViewModel viewModel)
    //        viewModel.AttachedToVisualTree(sender, e);
    //}

}