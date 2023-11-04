using Avalonia.Controls;
using Avalonia.Interactivity;
using Heating_Control_UI.ViewModels;

namespace Heating_Control_UI;

public partial class HeatingControlView : UserControl
{
    public HeatingControlView(HeatingControlViewModel heatingControlViewModel)
    {
        InitializeComponent();
        DataContext = heatingControlViewModel;
    }

    public HeatingControlView()
    {
        InitializeComponent();
        DataContext = new HeatingControlViewModel(null);
    }

    private void UserControl_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (this.DataContext is HeatingControlViewModel viewModel)
            viewModel.AttachedToVisualTree(sender, e);
    }

}