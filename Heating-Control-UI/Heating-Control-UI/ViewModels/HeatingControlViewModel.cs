using Heating_Control.Data;
using Heating_Control.ML;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace Heating_Control_UI.ViewModels;
public class HeatingControlViewModel : ViewModelBase
{
    private readonly IHeatingControlNeuralNetwork _heatingControlNeuralNetwork;
    private TrainingDataOptions lastTraining = new TrainingDataOptions();

    public HeatingControlViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        if (heatingControlNeuralNetwork is null) return;
        this.PropertyChanged += HeatingControlViewModel_PropertyChanged;
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        _heatingControlNeuralNetwork.Inizialize();
    }

    public HeatingControlViewModel()
    {
    }

    private void HeatingControlViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Calculate();
    }

    private ObservableCollection<int> _temperatures = new ObservableCollection<int>() { 20, 35, 47, 57, 68, 80 };
    public ObservableCollection<int> Temperatures
    {
        get => _temperatures;
        set => this.RaiseAndSetIfChanged(ref _temperatures, value);
    }

    private void Calculate()
    {
        _temperatures.Clear();

        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 20, PredictedOutdoorTemperature = 20, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, 90));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 10, PredictedOutdoorTemperature = 10, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, 90));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 0, PredictedOutdoorTemperature = 0, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, 90));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -10, PredictedOutdoorTemperature = -10, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, 90));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -20, PredictedOutdoorTemperature = -20, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, 90));
        _temperatures.Add((int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -30, PredictedOutdoorTemperature = -30, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, 90));

        
    }

    private int _preferredIndoorTemperature = 26;
    public int PreferredIndoorTemperature
    {
        get => _preferredIndoorTemperature;
        set => this.RaiseAndSetIfChanged(ref _preferredIndoorTemperature, value);
    }


    private string _stettings = "Settings";

    public string Settings
    {
        get => _stettings;
        set => this.RaiseAndSetIfChanged(ref _stettings, value);
    }

    public void ButtonAction()
    {
      
        Settings = $"{Settings}2";
        App.Navigator.Push<HeatingControlSettingsView>();
    }



}
