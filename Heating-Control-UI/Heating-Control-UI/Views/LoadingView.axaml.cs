using Avalonia.Controls;

namespace Heating_Control_UI;

public partial class LoadingView : UserControl
{
    public LoadingView()
    {
        InitializeComponent();
    }

    private void TemperaturProgressBarControl_Finished(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // TODO add Transition between screens.
        App.Navigator.Push<HeatingControlView>();
    }
}