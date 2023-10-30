using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Heating_Control_UI.ViewModels;

namespace Heating_Control_UI;

public partial class HeatingControlSettingsView : UserControl
{
    public HeatingControlSettingsView()
    {
        InitializeComponent();
        DataContext = new HeatingControlSettingsViewModel();
    }

    public HeatingControlSettingsView(HeatingControlSettingsViewModel heatingControlSettingsViewModel)
    {
        InitializeComponent();
        DataContext = heatingControlSettingsViewModel;
    }
}