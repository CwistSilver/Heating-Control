using Heating_Control_UI.ViewModels.Pages;

namespace Heating_Control_UI.Views.Pages;
public partial class HeatingControlSettingsView : PageControl
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