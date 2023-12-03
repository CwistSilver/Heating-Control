using DynamicData;
using Heating_Control.Data;
using Heating_Control.NeuralNetwork;
using Heating_Control_UI.Utilities.Navigation;
using Heating_Control_UI.Views.Pages;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Heating_Control_UI.ViewModels.Pages;
public class HeatingControlViewModel : ViewModelBase
{
    private ObservableCollection<float> _temperatures = new ObservableCollection<float>() { 20, 35, 47, 57, 68, 80 };
    public ObservableCollection<float> Temperatures
    {
        get => _temperatures;
        set => this.RaiseAndSetIfChanged(ref _temperatures, value);
    }

    private int _preferredIndoorTemperature = 26;
    public int PreferredIndoorTemperature
    {
        get => _preferredIndoorTemperature;
        set
        {
            App.Storage.AddOrSet(value);
            this.RaiseAndSetIfChanged(ref _preferredIndoorTemperature, value);
            Calculate();
        }
    }

    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork;
    public HeatingControlViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        Inizialize();
    }

    public HeatingControlViewModel() { }

    private void Inizialize()
    {
        if (_heatingControlNeuralNetwork is null) return;

        _preferredIndoorTemperature = App.Storage.Get(nameof(PreferredIndoorTemperature), 26);
        Calculate();
    }

    public void NavigatedTo()
    {
        LoadSetting();
        Inizialize();
    }

    public async void NavigateToDayView()
    {
        await App.Navigator.PushAsync<DayChart>(PageNavigator.DefaultVerticalSlideTransition);
        App.Navigator.DestroyPage<HeatingControlView>();
        SelectedMode = nameof(DayChart);
    }

    private void Calculate()
    {
        if (_heatingControlNeuralNetwork is null) return;
        _temperatures.Clear();

        var inputs = new List<HeatingControlInputData>
        {
            new() { OutdoorTemperature = 20, PredictedOutdoorTemperature = 20, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature },
            new() { OutdoorTemperature = 10, PredictedOutdoorTemperature = 10, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature },
            new() { OutdoorTemperature = 0, PredictedOutdoorTemperature = 0, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature },
            new() { OutdoorTemperature = -10, PredictedOutdoorTemperature = -10, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature },
            new() { OutdoorTemperature = -20, PredictedOutdoorTemperature = -20, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature },
            new() { OutdoorTemperature = -30, PredictedOutdoorTemperature = -30, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature }
        };

        var results = _heatingControlNeuralNetwork.Predict(inputs);
        var output = results.Select(r => (float)(int)r.SupplyTemperature);

        _temperatures.AddRange(output);
    }
}
