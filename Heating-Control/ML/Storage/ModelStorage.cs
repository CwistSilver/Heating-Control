using Heating_Control.Data;
using Heating_Control.NeuralNetwork.Model;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.Json;
using Tensorflow;
using static Tensorflow.Binding;

namespace Heating_Control.ML.Storage;
public sealed class ModelStorage : IModelStorage
{
    private const string FileName = "HeatingControl.zip";
    private const string OptionsFileName = "HeatingControl-Options.json";
    private readonly string _storagePath;
    private readonly string _optionsStoragePath;
    private readonly ILogger<IModelStorage> _logger;
    private readonly IModelCreator _modelCreator;
    private readonly int[] hiddenLayers = new int[3] { 5, 10, 5 };
    private readonly Saver _saver;

    public ModelStorage(ILogger<IModelStorage> logger, IModelCreator modelCreator)
    {
        _logger = logger;
        _modelCreator = modelCreator;
        _modelCreator.CreateModel();

        var directory = GetDataDirectorPath();
        _storagePath = Path.Combine(directory, FileName);
        _optionsStoragePath = Path.Combine(directory, OptionsFileName);

        _saver = tf.train.Saver();

        _logger.LogInformation("ModelStorage initialized. Model path: {0}, Options path: {1}", _storagePath, _optionsStoragePath);
    }

    public string GetDataDirectorPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control));
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

    public ModelData? Load(Session session)
    {
        try
        {
            _logger.LogInformation("Loading model from {0}", _storagePath);


            if (!File.Exists(_optionsStoragePath))
            {
                _logger.LogWarning("Options file not found at {0}", _optionsStoragePath);
                return null;
            }

            _saver.restore(session, _storagePath);

          

            var jsonTest = File.ReadAllText(_optionsStoragePath);
            var options = JsonSerializer.Deserialize<TrainingDataOptions>(jsonTest);
            if (options is null)
            {
                _logger.LogWarning("Failed to deserialize options from {0}", _optionsStoragePath);
                return null;
            }

            _logger.LogInformation("Model and options loaded successfully");
            return new ModelData() { Data = session, Options = options };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading the model");
            return null;
        }
    }
    public void Save(ModelData modelData)
    {
        _logger.LogInformation("Saving model to {0}", _storagePath);

        if (!Directory.Exists(Path.GetDirectoryName(_storagePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_storagePath)!);
            _logger.LogInformation("Created directory for model at {0}", _storagePath);
        }

        if (!Directory.Exists(Path.GetDirectoryName(_optionsStoragePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_optionsStoragePath)!);
            _logger.LogInformation("Created directory for options at {0}", _optionsStoragePath);
        }

        try
        {           
            _saver.save(modelData.Data, _storagePath);

            var jsonText = JsonSerializer.Serialize(modelData.Options, new JsonSerializerOptions() { WriteIndented = true });
            File.WriteAllText(_optionsStoragePath, jsonText);

            _logger.LogInformation("Model and options saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving the model");
        }
    }
}
