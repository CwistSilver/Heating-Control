using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Heating_Control.ML.DataProvider;
using Heating_Control.ML;
using Heating_Control;
using Microsoft.Extensions.DependencyInjection;
using Heating_Control.Data;
using System;

namespace Heating_Control_UI.Views;
public partial class MainView : UserControl
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IHeatingControlNeuralNetwork _heatingControlNeuralNetwork;

    public MainView()
    {
        InitializeComponent();
        var services = new ServiceCollection();
        HeatingControlEntry.ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _heatingControlNeuralNetwork = _serviceProvider.GetRequiredService<IHeatingControlNeuralNetwork>();
        _heatingControlNeuralNetwork.Inizialize();
        //var trainingDataProvider = serviceProvider.GetRequiredService<ITrainingDataProvider>();

    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        

        LineChartViewVar.Temperatures[0] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 20, PredictedOutdoorTemperature = 20, PreferredIndoorTemperature = 23 }).SupplyTemperature,0,90);
        LineChartViewVar.Temperatures[1] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 10, PredictedOutdoorTemperature = 10, PreferredIndoorTemperature = 23 }).SupplyTemperature,0,90);
        LineChartViewVar.Temperatures[2] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 0, PredictedOutdoorTemperature = 0, PreferredIndoorTemperature = 23 }).SupplyTemperature,0,90);
        LineChartViewVar.Temperatures[3] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -10, PredictedOutdoorTemperature = -10, PreferredIndoorTemperature = 23 }).SupplyTemperature,0,90);
        LineChartViewVar.Temperatures[4] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -20, PredictedOutdoorTemperature = -20, PreferredIndoorTemperature = 23 }).SupplyTemperature,0,90);
        LineChartViewVar.Temperatures[5] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -30, PredictedOutdoorTemperature = -30, PreferredIndoorTemperature = 23 }).SupplyTemperature,0,90);
    }
}