using Heating_Control;
using Heating_Control.Data.Training;
using Heating_Control.Training;
using Heating_Control.Training.DataProvider;
using Heating_Control.Training.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Heating_Control_Trainer;

internal class Program
{
    private static NeuralNetworkConfig NeuralNetwork1_2_1 = new()
    {
        Name = "Neural-Network-1-2-1",
        Layers = new List<LayerCreationData>()
            {
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 256},
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 128},
                new(){ ActivationFunction = ActivationFunction.Relu, Neurons = 64}
            },
        TrainingSize = 10_000,
        Epochs = 10,
        BatchSize = 100,
        ValidationDatas = 2000
    };


    static void PredictTest(INeuralNetworkConfigRunner neuralNetworkConfigRunner, NeuralNetworkConfig neuralNetworkConfig)
    {
        var inputs = TrainingDataProvider.GetStaticTestSet();

        for (int i = 0; i < inputs.Count; i++)
        {
            var prediction = neuralNetworkConfigRunner.Predict(neuralNetworkConfig, inputs[i]);
            Console.WriteLine($"Prediction for temp: {inputs[i].OutdoorTemperature}°C => {prediction}°C  | In Real: {inputs[i].SupplyTemperature}");
        }
    }

    static void Main(string[] args)
    {
        var services = new ServiceCollection();
        HeatingControlEntry.ConfigureServices(services);
        services.AddSingleton<IMultiLauncher, MultiLauncher>();

        var serviceProvider = services.BuildServiceProvider();
        var neuralNetworkConfigRunner = serviceProvider.GetRequiredService<INeuralNetworkConfigRunner>();
        var trainingStorage = serviceProvider.GetRequiredService<ITrainingStorage>();
        var multiLauncher = serviceProvider.GetRequiredService<IMultiLauncher>();

        //neuralNetworkConfigRunner.Train(NeuralNetwork1_2_1);
        //PredictTest(neuralNetworkConfigRunner, NeuralNetwork1_2_1);
        //return;

        if (args.Length == 0)
        {
            var configs = NeuralNetworkConfigs.GetNeuralNetworkConfigs();

            multiLauncher.Launch(configs);
        }

        if (args.Length > 1) { return; }

        if (args.Length == 1)
        {
            var neuralNetworkConfig = JsonSerializer.Deserialize<NeuralNetworkConfig>(args[0]);
            if (neuralNetworkConfig is null)
            {
                Console.WriteLine("NeuralNetworkConfig is null!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine(JsonSerializer.Serialize(neuralNetworkConfig));

            try
            {
                neuralNetworkConfigRunner.Train(neuralNetworkConfig);
            }
            catch (Exception ex)
            {
                var path = trainingStorage.GetErrorFilePath(neuralNetworkConfig);
                File.WriteAllText(path, ex.Message);
            }
        }
    }
}
