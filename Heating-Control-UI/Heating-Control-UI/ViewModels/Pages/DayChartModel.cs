using DynamicData;
using Heating_Control.Data;
using Heating_Control.NeuralNetwork;
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
    private const byte TimeSteps = 9;

    private float _maxTemperatur = 90f;
    public float MaxTemperatur
    {
        get => _maxTemperatur;
        set => this.RaiseAndSetIfChanged(ref _maxTemperatur, value);
    }

    private ObservableCollection<float> _temperatures = new();

    public ObservableCollection<float> Temperatures
    {
        get => _temperatures;
        set => this.RaiseAndSetIfChanged(ref _temperatures, value);
    }

    private ObservableCollection<float> _temperaturesToday = new();

    public ObservableCollection<float> TemperaturesToday
    {
        get => _temperaturesToday;
        set => this.RaiseAndSetIfChanged(ref _temperaturesToday, value);
    }

    private ObservableCollection<float> _times = new();
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
    private readonly WeatherClient _weatherClient = new(OpenWeatherMapApiKey);
    private bool isReady = false;

    public DayChartModel(IHeatingControlNeuralNetwork heatingControlNeuralNetwork)
    {
        _preferredIndoorTemperature = App.Storage.Get(nameof(PreferredIndoorTemperature), 26);
        _heatingControlNeuralNetwork = heatingControlNeuralNetwork;
        _ = GetWeatherData();

        TemperaturesToday.CollectionChanged += TemperaturesToday_CollectionChanged;
    }

    private void TemperaturesToday_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (!isReady) return;
        Calculate();
    }

    public DayChartModel() { InsertFakeData(); }

    private void InsertFakeData()
    {
        _times.Clear();
        _temperaturesToday.Clear();
        _temperatures.Clear();

        for (int i = 0; i < TimeSteps; i++)
        {
            _times.Add(i * 3);
            _temperaturesToday.Add(Random.Shared.Next(-5, 20));
            _temperatures.Add(Random.Shared.Next(-5, 20));
        }
    }

    public void NavigatedTo()
    {
        if (!isReady) return;
        LoadSetting();
        Calculate();
    }

    private void Calculate()
    {
        _temperatures.Clear();
        _temperatures.AddRange(PredictSupplyTemperatures());
    }

    private async Task GetWeatherData()
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

                if (_temperaturesToday.Count == TimeSteps) break;
            }

            Calculate();
            isReady = true;
        }
        catch
        {
            return;
        }
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

    private float[] PredictSupplyTemperatures()
    {
        var inputs = new HeatingControlInputData[_temperaturesToday.Count];
        for (int i = 0; i < _temperaturesToday.Count; i++)
        {
            var currentOutdoorTemperature = _temperaturesToday[i];
            inputs[i] = new HeatingControlInputData() { OutdoorTemperature = currentOutdoorTemperature, PredictedOutdoorTemperature = PredictedOutdoorTemperature, PreferredIndoorTemperature = _preferredIndoorTemperature, Baseline = Baseline, Gradient = Gradient, MaxSupplyTemperature = MaxSupplyTemperature };
        }

        var results = _heatingControlNeuralNetwork!.Predict(inputs);
        var output = new float[_temperaturesToday.Count];
        for (int i = 0; i < _temperaturesToday.Count; i++)
        {
            output[i] = (int)results[i].SupplyTemperature;
        }

        return output;
    }

    public async void SwitchSuplayView()
    {
        await App.Navigator.PushAsync<HeatingControlView>(PageNavigator.DefaultVerticalSlideTransition);
        App.Navigator.DestroyPage<DayChart>();
        SelectedMode = nameof(HeatingControlView);
    }

    private static int GetHour(DateTime dateTime) => dateTime.Hour / 3 * 3;
}
