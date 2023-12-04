using Heating_Control.Data.Training;
using Heating_Control.Training;
using Heating_Control.Training.Storage;
using System.Diagnostics;
using System.Text.Json;

namespace Heating_Control_Trainer;
public class MultiLauncher : IMultiLauncher
{
    private readonly List<NeuralNetworkConfig> _finishedNeuralNetworks = [];
    private readonly INeuralNetworkConfigRunner _neuralNetworkConfigRunner;
    private readonly ITrainingStorage _trainingStorage;
    public MultiLauncher(INeuralNetworkConfigRunner neuralNetworkConfigRunner, ITrainingStorage trainingStorage)
    {
        _neuralNetworkConfigRunner = neuralNetworkConfigRunner;
        _trainingStorage = trainingStorage;
    }

    public void Launch(List<NeuralNetworkConfig> configs)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < configs.Count; i++)
        {
            int index = i;
            var task = Task.Run(() => LaunchProcess(configs[index]));
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());


        Console.WriteLine($"{Environment.NewLine}");
        Console.WriteLine("All Neural Network are trained!");

        var bestNetwork = FindBestTrainedNetwork();
        if (bestNetwork != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The best Neural Network is '{bestNetwork.Name}'!");
            Console.WriteLine($"With a Average precision: {bestNetwork.Results!.Average}");
            Console.WriteLine($"{_trainingStorage.GetModelConfigDataFilePath(bestNetwork)}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No Neural Network found.");
            Console.ResetColor();
        }

        // TODO Finde heraus welches da best trinierte neural netzer ist das inder liste "_finishedNeuralNetworks" steht. wie gut es trainiert ist sieht man an den ModelResult.
    }

    private NeuralNetworkConfig? FindBestTrainedNetwork()
    {
        NeuralNetworkConfig? bestNetwork = null;
        float bestAverage = float.MaxValue;

        foreach (var network in _finishedNeuralNetworks)
        {
            if (network.Results is not null && network.Results.Average < bestAverage)
            {
                bestAverage = network.Results.Average;
                bestNetwork = network;
            }
        }

        return bestNetwork;
    }


    private Process CreateProcess(NeuralNetworkConfig config)
    {
        var exeName = $"Heating-Control-Trainer.exe";
        var exePath = Path.Combine(Directory.GetCurrentDirectory(), exeName);

        var jsonMessage = JsonSerializer.Serialize(config, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        var arguments = jsonMessage.Replace("\"", "\\\"");

        var process = new Process();
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = true;
        return process;
    }

    private const int maxTrys = 51;
    private void LaunchProcess(NeuralNetworkConfig config, int currentTry = 0)
    {
        if (currentTry == maxTrys)
            return;

        if (currentTry != 0)
            Console.WriteLine($"try {currentTry}/{maxTrys - 1} for {config.Name}");

        var process = CreateProcess(config);
        process.Start();

        process.WaitForExit();

        var state = _trainingStorage.LoadState(config);
        if (state is not null)
        {
            if (!state.Finished)
            {
                process.Dispose();
                Console.WriteLine($"Process failed for {config.Name}, try again.");
                LaunchProcess(config, currentTry + 1);

                if (currentTry != 0) return;
            }
        }

        var jsonPath = _trainingStorage.GetModelConfigDataFilePath(config);
        if (!File.Exists(jsonPath))
        {
            var missingJson = JsonSerializer.Serialize(config, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
            Console.WriteLine("File not found for:");
            Console.WriteLine(missingJson);
            Console.WriteLine($"{Environment.NewLine}");
            return;
        }

        var jsonFileContent = File.ReadAllText(jsonPath);
        var newConfig = JsonSerializer.Deserialize<NeuralNetworkConfig>(jsonFileContent);
        if (newConfig is null)
        {
            var missingJson = JsonSerializer.Serialize(config, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
            Console.WriteLine("File could not be loaded.");
            Console.WriteLine(missingJson);
            Console.WriteLine($"{Environment.NewLine}");
            return;
        }

        var newConfigJson = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Neural Network Finished with:");
        Console.ResetColor();
        Console.WriteLine(newConfigJson);
        Console.WriteLine($"{Environment.NewLine}");
        _finishedNeuralNetworks.Add(newConfig);
    }

}