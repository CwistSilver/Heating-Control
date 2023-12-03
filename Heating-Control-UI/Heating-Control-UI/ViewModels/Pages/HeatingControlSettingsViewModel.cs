using Heating_Control_UI.Utilities.Navigation;
using ReactiveUI;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels.Pages;
public class HeatingControlSettingsViewModel : ViewModelBase
{
    private float _newMaxSupplyTemperature = 90f;
    public float NewMaxSupplyTemperature
    {
        get => _newMaxSupplyTemperature;
        set => this.RaiseAndSetIfChanged(ref _newMaxSupplyTemperature, value);
    }
    private float _newGradient = 1.5f;
    public float NewGradient
    {
        get => _newGradient;
        set => this.RaiseAndSetIfChanged(ref _newGradient, value);
    }

    private float _newBaseline = 1.5f;
    public float NewBaseline
    {
        get => _newBaseline;
        set => this.RaiseAndSetIfChanged(ref _newBaseline, value);
    }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => this.RaiseAndSetIfChanged(ref _isSaving, value);
    }

    public HeatingControlSettingsViewModel()
    {
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        NewBaseline = Baseline;
        NewGradient = Gradient;
        NewMaxSupplyTemperature = MaxSupplyTemperature;

    }

    public async void CancelAction() => await App.Navigator.PopAsync(PageNavigator.DefaultHorizontalSlideTransition);
    public async void Save()
    {
        IsSaving = true;
        try
        {
            MaxSupplyTemperature = NewMaxSupplyTemperature;
            Baseline = NewBaseline;
            Gradient = NewGradient;

            var tasks = new Task[2]
            {
                Task.Delay(3_000),
                App.Storage.Save()
            };
            await Task.WhenAll(tasks);

            await App.Navigator.PopAsync(PageNavigator.DefaultHorizontalSlideTransition);
        }
        finally
        {
            IsSaving = false;
        }
    }
}

