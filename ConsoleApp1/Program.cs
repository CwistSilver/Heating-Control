using Heating_Control;
using Heating_Control.Data;
using Heating_Control.ML.DataProvider;
using Heating_Control.NeuralNetwork;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp1;

internal class Program
{
    static ServiceProvider serviceProvider;
    static async Task<IReadOnlyList<HeatingControlTrainingData>> GetTraingData(uint rec = 10_000)
    {
        var trainingDataProvider = serviceProvider.GetRequiredService<ITrainingDataProvider>();
        return await trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = rec });

    }
    static async Task Main(string[] args)
    {
        //var directory = GetDataDirectorPath();
        //_storagePath = Path.Combine(directory, FileName);

        Console.WriteLine("Hello, World!");
        var services = new ServiceCollection();
        HeatingControlEntry.ConfigureServices(services);

        serviceProvider = services.BuildServiceProvider();

        var trainingData = await GetTraingData();
        var testDatas = await GetTraingData(20);

        var trainingDataProvider = serviceProvider.GetRequiredService<ITrainingDataProvider>();
        var tensorflowNeuralNetwork = serviceProvider.GetRequiredService<TensorflowNeuralNetwork>();
        //tensorflowNeuralNetwork.Load();
        await tensorflowNeuralNetwork.Train(trainingDataProvider);
        tensorflowNeuralNetwork.Save();

        foreach (var data in testDatas)
        {
            Console.WriteLine();
            var prediction = tensorflowNeuralNetwork.Predict(new HeatingControlInputData() { OutdoorTemperature = data.OutdoorTemperature, PredictedOutdoorTemperature = data.PredictedOutdoorTemperature, PreferredIndoorTemperature = data.PreferredIndoorTemperature });
            Console.WriteLine($"Vorhergesagte Vorlauftemperatur für Testdaten: {prediction.SupplyTemperature}");
            Console.WriteLine($"Tatsächliche Vorlauftemperatur der Testdaten: {data.SupplyTemperature}");
        }

    }

}
