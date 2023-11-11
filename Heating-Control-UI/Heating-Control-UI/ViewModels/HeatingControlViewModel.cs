using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control_UI.Utilities;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels;
public class HeatingControlViewModel : ViewModelBase
{
    private readonly IHeatingControlNeuralNetwork _heatingControlNeuralNetwork;
    public HeatingControlViewModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        if (heatingControlNeuralNetwork is null) return;
        this.PropertyChanged += HeatingControlViewModel_PropertyChanged;
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        Inizialize();
    }

    private void Inizialize()
    {
        MaxTemperatur = _heatingControlNeuralNetwork.UsedTrainingDataOptions.MaxSupplyTemperature;
        Calculate();
    }

    public HeatingControlViewModel()
    {
    }

    private void HeatingControlViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Calculate();
        // TODO save to App settings when changed!
    }

    private float _maxTemperatur = 90f;
    public float MaxTemperatur
    {
        get => _maxTemperatur;
        set => this.RaiseAndSetIfChanged(ref _maxTemperatur, value);
    }

    private ObservableCollection<float> _temperatures = new ObservableCollection<float>() { 20, 35, 47, 57, 68, 80 };
    public ObservableCollection<float> Temperatures
    {
        get => _temperatures;
        set => this.RaiseAndSetIfChanged(ref _temperatures, value);
    }

    public void NavigatedTo()
    {
        MaxTemperatur = _heatingControlNeuralNetwork.UsedTrainingDataOptions.MaxSupplyTemperature;
        Calculate();
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

    private int _preferredIndoorTemperature = 26;
    public int PreferredIndoorTemperature
    {
        get => _preferredIndoorTemperature;
        set => this.RaiseAndSetIfChanged(ref _preferredIndoorTemperature, value);
    }



    public void ButtonAction()
    {
        App.Navigator.Push<HeatingControlSettingsView>(PageNavigator.DefaultSlideTransition);
    }

    public void SwitchDayView()
    {

        var pageTransition = new PageSlide(TimeSpan.FromMilliseconds(1_000),PageSlide.SlideAxis.Vertical);
        pageTransition.SlideOutEasing = new SineEaseInOut();
        pageTransition.SlideInEasing = new SineEaseInOut();
        App.Navigator.Push<DayChart>(pageTransition);
    }



}
