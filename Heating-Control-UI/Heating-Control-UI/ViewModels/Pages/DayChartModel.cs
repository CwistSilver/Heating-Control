using DynamicData;
using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control_UI.Utilities.Navigation;
using Heating_Control_UI.Views.Pages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Weather.NET;
using Weather.NET.Enums;
using Weather.NET.Models.WeatherModel;

namespace Heating_Control_UI.ViewModels.Pages;
public class DayChartModel : ViewModelBase
{
    private const string OpenWeatherMapApiKey = "d211601432e5a3ae70e858c167a94064";
    private const Measurement MetricMeasurement = Measurement.Metric;
    private const string CityName = "Krefeld";
    private const byte MaxRecords = 9;

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
            if (!isReady) return;
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
            if (!isReady) return;
            Calculate();
        }
    }

    private readonly IHeatingControlNeuralNetwork? _heatingControlNeuralNetwork = null;
    private readonly WeatherClient _weatherClient = new WeatherClient(OpenWeatherMapApiKey);
    private bool isReady = false;

    public DayChartModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        _preferredIndoorTemperature = App.Storage.Get(nameof(PreferredIndoorTemperature), 26);
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        _ = SetRealToday();
    }

    public DayChartModel() { InsertFakeData(); }

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

    private static int CalculateTemperatureTomorrow(List<WeatherModel> weatherForecast)
    {
        int totalRecords = 0;
        double totalTemperature = 0;
        var timeNow = DateTime.Now;

        foreach (var weather in weatherForecast)
        {
            var currentTimeSpan = weather.AnalysisDate - timeNow;
            if (currentTimeSpan >= TimeSpan.FromHours(24) && currentTimeSpan <= TimeSpan.FromHours(48))
            {
                totalRecords++;
                totalTemperature += weather.Main.Temperature;
            }
        }

        return (int)totalTemperature / totalRecords;
    }

    private async Task SetRealToday()
    {
        try
        {
            var weatherForecast = await _weatherClient.GetForecastAsync(CityName, measurement: MetricMeasurement);          
            var currentWeather = await _weatherClient.GetCurrentWeatherAsync(CityName, measurement: MetricMeasurement);
            PredictedOutdoorTemperature = CalculateTemperatureTomorrow(weatherForecast);

            _temperaturesToday.Clear();
            _times.Clear();

            _temperaturesToday.Add((int)currentWeather.Main.Temperature);
            _times.Add(GetHour(DateTime.Now.ToLocalTime()));

            foreach (var weather in weatherForecast)
            {
                var time = weather.AnalysisDate;
                if (_times[0] == time.Hour && time.Day == DateTime.Today.Day)
                {
                    _temperaturesToday[0] = (int)weather.Main.Temperature;
                    continue;
                }
                _times.Add(time.Hour);
                _temperaturesToday.Add((int)weather.Main.Temperature);

                if (_temperaturesToday.Count == MaxRecords) break;
            }

            Calculate();
            isReady = true;
        }
        catch
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

    private static int GetHour(DateTime dateTime) => dateTime.Hour / 3 * 3;
}
