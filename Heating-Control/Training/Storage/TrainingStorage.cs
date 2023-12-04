using Heating_Control.Data.Training;
using System.Runtime.InteropServices;
using System.Text.Json;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;

namespace Heating_Control.Training.Storage;
public class TrainingStorage : ITrainingStorage
{
    private const string ModelFileName = "KerasModel.h5";
    private const string ModelConfigDataFileName = "Model-Config-Data.json";

    private readonly string _rootDirectory;
    public TrainingStorage()
    {
        _rootDirectory = GetDataDirectorPath();
    }

    public string GetDataDirectorPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control), "Training");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control), "Training");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }

    public string GetModelFilePath(NeuralNetworkConfig config)
    {
        var directoryPath = Path.Combine(_rootDirectory, config.Name);
        var modelDirectoryPath = Path.Combine(directoryPath, "Model");
        var modelPath = Path.Combine(modelDirectoryPath, ModelFileName);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        if (!Directory.Exists(modelDirectoryPath))
            Directory.CreateDirectory(modelDirectoryPath);

        return modelPath;
    }

    public void SaveState(NeuralNetworkConfig config, TrainingState trainingState)
    {
        var path = GetStateFilePath(config);

        var josnText = JsonSerializer.Serialize(trainingState);
        File.WriteAllText(path, josnText);
    }

    public TrainingState? LoadState(NeuralNetworkConfig config)
    {
        var path = GetStateFilePath(config);
        if (!File.Exists(path))
            return null;

        var josnText = File.ReadAllText(path);
        if (josnText is null) return null;
        return JsonSerializer.Deserialize<TrainingState>(josnText);
    }

    public string GetStateFilePath(NeuralNetworkConfig config)
    {
        var directoryPath = Path.Combine(_rootDirectory, config.Name);

        var modelPath = Path.Combine(directoryPath, "state.json");

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        return modelPath;
    }

    public string GetModelConfigDataFilePath(NeuralNetworkConfig config)
    {
        var directoryPath = Path.Combine(_rootDirectory, config.Name);
        var modelPath = Path.Combine(directoryPath, ModelConfigDataFileName);

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        return modelPath;
    }

    public string GetErrorFilePath(NeuralNetworkConfig config)
    {
        var directoryPath = Path.Combine(_rootDirectory, config.Name);
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        var errorFileName = "ERROR_LOG";
        var currentFileName = errorFileName;
        var errorFilePath = Path.Combine(directoryPath, $"{currentFileName}.txt");

        int trys = 1;
        while (File.Exists(errorFilePath))
        {
            currentFileName = $"{errorFileName}({trys})";
            errorFilePath = Path.Combine(directoryPath, $"{currentFileName}.txt");
            trys++;
        }

        return errorFilePath;
    }

    public IModel Load(NeuralNetworkConfig config)
    {
        try
        {
            var model = keras.models.load_model(GetModelFilePath(config));
            return model;
        }
        catch
        {
            return null;
        }
    }
}
