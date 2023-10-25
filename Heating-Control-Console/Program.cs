using Heating_Control;
using Heating_Control.Data;
using Heating_Control.ML;
using Heating_Control.ML.DataProvider;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Heating_Control_Console;
internal class Program
{
    static ServiceCollection services = new ServiceCollection();
    static async Task Main(string[] args)
    {


        HeatingControlEntry.ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        var heatingControlNeuralNetwork = serviceProvider.GetRequiredService<IHeatingControlNeuralNetwork>();
        var trainingDataProvider = serviceProvider.GetRequiredService<ITrainingDataProvider>();

        await heatingControlNeuralNetwork.Inizialize();
        var trainingDatas = await trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = 20 });
        foreach (var trainingData in trainingDatas)
        {
            var input = new HeatingControlInputData() { OutdoorTemperature = trainingData.OutdoorTemperature, PredictedOutdoorTemperature = trainingData.PredictedOutdoorTemperature, PreferredIndoorTemperature = trainingData.PreferredIndoorTemperature };
            Console.WriteLine($"Get Temperature for: {JsonSerializer.Serialize(input)}");
            var prediction = heatingControlNeuralNetwork.Predict(input);
            Console.WriteLine($"SupplyTemperature: {trainingData.SupplyTemperature} | SupplyTemperature (AI prediction): {prediction.SupplyTemperature}");
            Console.WriteLine();
        }
    }
}