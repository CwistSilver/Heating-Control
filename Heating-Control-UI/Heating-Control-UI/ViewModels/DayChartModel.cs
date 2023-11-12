using Avalonia.Animation;
using Avalonia.Animation.Easings;
using DynamicData;
using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control_UI.Utilities;
using OpenWeatherMap;
using OpenWeatherMap.Util;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels;
public class DayChartModel : ViewModelBase
{
    private const string ApiKey = "d211601432e5a3ae70e858c167a94064";
    private const string CityName = "Krefeld";
    private const int MaxRecords = 9;

    private float _maxTemperatur = 90f;
    public float MaxTemperatur
    {
        get => _maxTemperatur;
        set => this.RaiseAndSetIfChanged(ref _maxTemperatur, value);
    }

    private ObservableCollection<float> _temperatures = new ObservableCollection<float>();

    public ObservableCollection<float> Temperatures
    {
        get => _temperatures;
        set => this.RaiseAndSetIfChanged(ref _temperatures, value);
    }

    private ObservableCollection<float> _temperaturesToday = new ObservableCollection<float>();

    public ObservableCollection<float> TemperaturesToday
    {
        get => _temperaturesToday;
        set => this.RaiseAndSetIfChanged(ref _temperaturesToday, value);
    }

    private ObservableCollection<float> _times = new ObservableCollection<float>();
    public ObservableCollection<float> Times
    {
        get => _times;
        set => this.RaiseAndSetIfChanged(ref _times, value);
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

    private int _predictedOutdoorTemperature = 26;
    public int PredictedOutdoorTemperature
    {
        get => _predictedOutdoorTemperature;
        set
        {
            this.RaiseAndSetIfChanged(ref _predictedOutdoorTemperature, value);
            Calculate();
        }
    }

    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork = null;
    private readonly OpenWeatherMapService _openWeatherMapService = new(new OpenWeatherMapOptions { ApiKey = ApiKey });
    private bool isReady = false;

    public DayChartModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        _preferredIndoorTemperature = App.Storage.Get<int>(nameof(PreferredIndoorTemperature), 26);

        RequestOptions.Default.Unit = UnitType.Metric;
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        _ = SetRealToday();
    }

    public DayChartModel()
    {
        InsertFakeData();
    }
    
    private void InsertFakeData()
    {
        _times.Clear();
        _temperaturesToday.Clear();
        _temperatures.Clear();

        for (int i = 0; i < MaxRecords; i++)
        {
            _times.Add(i * 3);
            _temperaturesToday.Add(Random.Shared.Next(-5, 20));
            _temperatures.Add(Random.Shared.Next(-5, 20));
        }
    }

    public void NavigatedTo()
    {
        if (!isReady) return;
        Calculate();
    }

    private void Calculate()
    {
        _temperatures.Clear();
        var newtemperatures = new float[MaxRecords];
        for (int i = 0; i < MaxRecords; i++)
        {
            newtemperatures[i] = PredictSupplyTemperature(i);
        }

        _temperatures.AddRange(newtemperatures);
    }

    private async Task SetRealToday()
    {
        try
        {
            var weatherForecast = await _openWeatherMapService.GetWeatherForecastAsync(CityName);
            var nextDay = weatherForecast.List.Where(weather => (DateTime.Now - weather.CalculationTime!.Value.LocalDateTime) >= TimeSpan.FromHours(24));
            var currentWeather = await _openWeatherMapService.GetCurrentWeatherAsync(CityName);

            _temperaturesToday.Clear();
            _times.Clear();
            var timee = DateTime.Now.ToLocalTime();
            var h = GetHour(timee);
            _temperaturesToday.Add((int)currentWeather.Temperature.Value);
            _times.Add(GetHour(DateTime.Now.ToLocalTime()));

            foreach (var weather in weatherForecast.List)
            {
                var time = weather.CalculationTime;
                _times.Add(time!.Value.Hour);
                _temperaturesToday.Add((int)weather.Temperature.Value);

                if (_times.Count == MaxRecords) break;
            }

            Calculate();
            isReady = true;
        }
        catch (Exception ex)
        {
            return;
        }
    }

    private float PredictSupplyTemperature(int index)
    {
        if (_heatingControlNeuralNetwork is null) return 0f;

        var currentOutdoorTemperature = _temperaturesToday[index];
        return (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = currentOutdoorTemperature, PredictedOutdoorTemperature = PredictedOutdoorTemperature, PreferredIndoorTemperature = _preferredIndoorTemperature }).SupplyTemperature, 0, MaxTemperatur);
    }

    public async void SwitchSuplayView()
    {        
        await App.Navigator.PushAsync<HeatingControlView>(PageNavigator.DefaultVerticalSlideTransition);
        App.Navigator.DestroyPage<DayChart>();
        SelectedMode = nameof(HeatingControlView);
    }

    private static int GetHour(DateTime dateTime) => (dateTime.Hour / 3) * 3;
}
