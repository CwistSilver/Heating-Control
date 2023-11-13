using Heating_Control_UI.Utilities.Navigation;
using Heating_Control_UI.Views.Pages;
using ReactiveUI;

namespace Heating_Control_UI.ViewModels;
public class ViewModelBase : ReactiveObject
{
    private string _selectedMode = nameof(HeatingControlView);
    public string SelectedMode
    {
        get => _selectedMode;
        set
        {
            App.Storage.AddOrSet(value);
            this.RaiseAndSetIfChanged(ref _selectedMode, value);
        }
    }

    public void NavigateToSettings()
    {
        App.Navigator.Push<HeatingControlSettingsView>(PageNavigator.DefaultHorizontalSlideTransition);
    }
}
