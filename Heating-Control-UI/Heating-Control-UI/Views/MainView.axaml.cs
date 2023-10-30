using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Heating_Control.ML.DataProvider;
using Heating_Control.ML;
using Heating_Control;
using Microsoft.Extensions.DependencyInjection;
using Heating_Control.Data;
using System;
using System.Threading.Tasks;

namespace Heating_Control_UI.Views;
public partial class MainView : UserControl
{
    //private readonly ServiceProvider _serviceProvider;
    //private readonly IHeatingControlNeuralNetwork _heatingControlNeuralNetwork;
    //private TrainingDataOptions lastTraining = new TrainingDataOptions();

    public MainView()
    {
        InitializeComponent();
        //var services = new ServiceCollection();
        //HeatingControlEntry.ConfigureServices(services);
        //_serviceProvider = services.BuildServiceProvider();
        //_heatingControlNeuralNetwork = _serviceProvider.GetRequiredService<IHeatingControlNeuralNetwork>();
        //_heatingControlNeuralNetwork.Inizialize();
        
        //var trainingDataProvider = serviceProvider.GetRequiredService<ITrainingDataProvider>();

    }

    //private async Task ReTrain()
    //{
    //    if (string.IsNullOrEmpty(NiveauTextBox.Text) || string.IsNullOrEmpty(NeigungTextBox.Text))
    //        return;



    //    var niveau = float.Parse(NiveauTextBox.Text);
    //    var neigung = float.Parse(NeigungTextBox.Text);

    //    if (lastTraining.Baseline == niveau && lastTraining.Gradient == neigung) return;

    //    lastTraining = new TrainingDataOptions() { Baseline = niveau, Gradient = neigung, RecordsToGenerate = 2_000_000 };
    //    await _heatingControlNeuralNetwork.Inizialize(lastTraining, true);
    //}

    //private async void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    //{
    //    await ReTrain();

    //    var temp = float.Parse(TempTextBox.Text);

    //    LineChartViewVar.Temperatures[0] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 20, PredictedOutdoorTemperature = 20, PreferredIndoorTemperature = temp }).SupplyTemperature,0,90);
    //    LineChartViewVar.Temperatures[1] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 10, PredictedOutdoorTemperature = 10, PreferredIndoorTemperature = temp }).SupplyTemperature,0,90);
    //    LineChartViewVar.Temperatures[2] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = 0, PredictedOutdoorTemperature = 0, PreferredIndoorTemperature = temp }).SupplyTemperature,0,90);
    //    LineChartViewVar.Temperatures[3] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -10, PredictedOutdoorTemperature = -10, PreferredIndoorTemperature = temp }).SupplyTemperature,0,90);
    //    LineChartViewVar.Temperatures[4] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -20, PredictedOutdoorTemperature = -20, PreferredIndoorTemperature = temp }).SupplyTemperature,0,90);
    //    LineChartViewVar.Temperatures[5] = (int)Math.Clamp(_heatingControlNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = -30, PredictedOutdoorTemperature = -30, PreferredIndoorTemperature = temp }).SupplyTemperature,0,90);
    //}
}