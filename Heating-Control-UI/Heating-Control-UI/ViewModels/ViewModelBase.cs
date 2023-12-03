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

    private float _baseline = 1.5f;
    public float Baseline
    {
        get => _baseline;
        set
        {
            App.Storage.AddOrSet(value);
            this.RaiseAndSetIfChanged(ref _baseline, value);
        }
    }

    private float _gradient = 1.5f;
    public float Gradient
    {
        get => _gradient;
        set
        {
            App.Storage.AddOrSet(value);
            this.RaiseAndSetIfChanged(ref _gradient, value);
        }
    }

    private float _maxSupplyTemperature = 90f;
    public float MaxSupplyTemperature
    {
        get => _maxSupplyTemperature;
        set
        {
            App.Storage.AddOrSet(value);
            this.RaiseAndSetIfChanged(ref _maxSupplyTemperature, value);
        }
    }

    public ViewModelBase()
    {
        _selectedMode = App.Storage.Get(nameof(SelectedMode), _selectedMode)!;
        LoadSetting();
    }

    public void LoadSetting()
    {
        MaxSupplyTemperature = App.Storage.Get(nameof(MaxSupplyTemperature), _maxSupplyTemperature);
        Gradient = App.Storage.Get(nameof(Gradient), _gradient);
        Baseline = App.Storage.Get(nameof(Baseline), _baseline);
    }

    public void NavigateToSettings()
    {
        App.Navigator.Push<HeatingControlSettingsView>(PageNavigator.DefaultHorizontalSlideTransition);
    }
}
