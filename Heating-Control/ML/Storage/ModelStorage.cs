using Heating_Control.Data;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Heating_Control.ML.Storage;
public sealed class ModelStorage : IModelStorage
{
    private const string FileName = "HeatingControl.zip";
    private const string OptionsFileName = "HeatingControl-Options.json";
    private readonly string _storagePath;
    private readonly string _optionsStoragePath;
    private readonly ILogger<IModelStorage> _logger;
    public ModelStorage(ILogger<IModelStorage> logger)
    {
        var directory = GetDataDirectorPath();
        _storagePath = Path.Combine(directory, FileName);
        _optionsStoragePath = Path.Combine(directory, OptionsFileName);
        _logger = logger;

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

    public ModelData? Load()
    {
        try
        {
            _logger.LogInformation("Loading model from {0}", _storagePath);

            if (!File.Exists(_storagePath))
            {
                _logger.LogWarning("Model file not found at {0}", _storagePath);
                return null;
            }

            if (!File.Exists(_optionsStoragePath))
            {
                _logger.LogWarning("Options file not found at {0}", _optionsStoragePath);
                return null;
            }

            var context = new MLContext();
            var transformer = context.Model.Load(_storagePath, out _);

            var jsonTest = File.ReadAllText(_optionsStoragePath);
            var options = JsonSerializer.Deserialize<TrainingDataOptions>(jsonTest);
            if (options is null)
            {
                _logger.LogWarning("Failed to deserialize options from {0}", _optionsStoragePath);
                return null;
            }

            _logger.LogInformation("Model and options loaded successfully");
            return new ModelData() { Transformer = transformer, Options = options };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading the model");
            return null;
        }
    }
    public void Save(ModelData modelData, DataViewSchema inputSchema)
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
            var context = new MLContext();
            context.Model.Save(modelData.Transformer, inputSchema, _storagePath);

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
