using DynamicData;
using Heating_Control.Data;
using Heating_Control.ML;
using OpenWeatherMap;
using OpenWeatherMap.Util;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels;
public class DayChartModel : ViewModelBase
{
    private const string ApiKey = "d211601432e5a3ae70e858c167a94064";
    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork = null;
    public DayChartModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        if (heatingControlNeuralNetwork is null) return;
        this.PropertyChanged += HeatingControlViewModel_PropertyChanged;
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        Inizialize();
        SetRealToday();
    }

    private void Inizialize()
    {
        //_heatingControlNeuralNetwork.UsedTrainingDataOptions.PredictedOutdoorTemperatureOffsetRange
        //MaxTemperatur = _heatingControlNeuralNetwork.UsedTrainingDataOptions.MaxSupplyTemperature;
        //Calculate();
        Calculate();

    }

    public DayChartModel()
    {
        this.PropertyChanged += HeatingControlViewModel_PropertyChanged;
        //_heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        Inizialize();
    }

    private void HeatingControlViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Calculate();
        // TODO save to App settings when changed!
    }

    private void Calculate()
    {

        _temperatures.Clear();

        SetToday();


        if (_heatingControlNeuralNetwork is null)
        {
            _temperatures.Add(1);
            _temperatures.Add(3);
            _temperatures.Add(6);
            _temperatures.Add(9);
            _temperatures.Add(12);
            _temperatures.Add(15);
            _temperatures.Add(18);
            _temperatures.Add(21);
            _temperatures.Add(24);
        }
        else
        {
            _temperatures.Add(Predict(0));
            _temperatures.Add(Predict(1));
            _temperatures.Add(Predict(2));
            _temperatures.Add(Predict(3));
            _temperatures.Add(Predict(4));
            _temperatures.Add(Predict(5));
            _temperatures.Add(Predict(6));
            _temperatures.Add(Predict(7));
            _temperatures.Add(Predict(8));

        }
    }

    private async Task SetRealToday()
    {
        var service = new OpenWeatherMapService(new OpenWeatherMapOptions
        {
            ApiKey = ApiKey
        });
        RequestOptions.Default.Unit = UnitType.Metric;

        try
        {
            var weather = await service.GetWeatherForecastAsync("Krefeld", forecastType:ForecastType.ThreeHour);
            var weather2 = await service.GetCurrentWeatherAsync("Krefeld");

            foreach (var item in weather.List)
            {
                var time = item.CalculationTime;
                int i = 0;
            }
            //var weather = await service.GetCurrentWeatherAsync("Krefeld");
            //var query = await openWeatherAPI.QueryAsync("city/krefeld");
            _temperaturesToday.Clear();
            _temperaturesToday.Add(6);
            _temperaturesToday.Add(10);
            _temperaturesToday.Add(13);
            _temperaturesToday.Add(15);
            _temperaturesToday.Add(18);
            _temperaturesToday.Add(12);
            _temperaturesToday.Add(11);
            _temperaturesToday.Add(8);
            _temperaturesToday.Add(4);

        }
        catch (Exception ex)
        {
            return;
        }



    }

    private void SetToday()
    {
        _temperaturesToday.Clear();
        _temperaturesToday.Add(6);
        _temperaturesToday.Add(10);
        _temperaturesToday.Add(13);
        _temperaturesToday.Add(15);
        _temperaturesToday.Add(18);
        _temperaturesToday.Add(12);
        _temperaturesToday.Add(11);
        _temperaturesToday.Add(8);
        _temperaturesToday.Add(4);
    }

    private float Predict(int index)
    {
        var currentOutdoorTemperature = _temperaturesToday[index];
        return (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = currentOutdoorTemperature, PredictedOutdoorTemperature = PredictedOutdoorTemperature, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur);
    }

    private float _maxTemperatur = 90f;
    public float MaxTemperatur
    {
        get => _maxTemperatur;
        set => this.RaiseAndSetIfChanged(ref _maxTemperatur, value);
    }

    private ObservableCollection<float> _temperatures = new ObservableCollection<float>() {
        6,
        10,
        13,
        15,
        18,
        12,
        11,
        8,
        4 };

    public ObservableCollection<float> Temperatures
    {
        get => _temperatures;
        set => this.RaiseAndSetIfChanged(ref _temperatures, value);
    }

    private ObservableCollection<float> _temperaturesToday = new ObservableCollection<float>() {
        10,
        30,
        60,
        90,
        120,
        150,
        180,
        210,
        240 };

    public ObservableCollection<float> TemperaturesToday
    {
        get => _temperaturesToday;
        set => this.RaiseAndSetIfChanged(ref _temperaturesToday, value);
    }

    private int _preferredIndoorTemperature = 26;
    public int PreferredIndoorTemperature
    {
        get => _preferredIndoorTemperature;
        set => this.RaiseAndSetIfChanged(ref _preferredIndoorTemperature, value);
    }

    private int _predictedOutdoorTemperature = 26;
    public int PredictedOutdoorTemperature
    {
        get => _predictedOutdoorTemperature;
        set => this.RaiseAndSetIfChanged(ref _predictedOutdoorTemperature, value);
    }
}
