using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control_UI.Utilities;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace Heating_Control_UI.ViewModels;
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

    private float _maxTemperatur = 90f;
    public float MaxTemperatur
    {
        get => _maxTemperatur;
        set => this.RaiseAndSetIfChanged(ref _maxTemperatur, value);
    }

    private readonly IHeatingControlNeuralNetwork _heatingControlNeuralNetwork;
    public HeatingControlViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        Inizialize();
    }

    private void Inizialize()
    {
        _preferredIndoorTemperature = App.Storage.Get<int>(nameof(PreferredIndoorTemperature), 26);
        MaxTemperatur = _heatingControlNeuralNetwork.UsedTrainingDataOptions.MaxSupplyTemperature;
        Calculate();
    }

    public HeatingControlViewModel()
    {
    }



    public void NavigatedTo()
    {
        Inizialize();
    }

    private void Calculate()
    {
        _temperatures.Clear();

        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 20, PredictedOutdoorTemperature = 20, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 10, PredictedOutdoorTemperature = 10, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 0, PredictedOutdoorTemperature = 0, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -10, PredictedOutdoorTemperature = -10, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -20, PredictedOutdoorTemperature = -20, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -30, PredictedOutdoorTemperature = -30, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur));


    }




    public async void NavigateToDayView()
    {
        await App.Navigator.PushAsync<DayChart>(PageNavigator.DefaultVerticalSlideTransition);
        App.Navigator.DestroyPage<HeatingControlView>();
        SelectedMode = nameof(DayChart);
    }
}
