using Heating_Control.Data;
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
    public ModelStorage()
    {
        var directory = GetDataDirectorPath();
        _storagePath = Path.Combine(directory, FileName);
        _optionsStoragePath = Path.Combine(directory, OptionsFileName);
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
            var context = new MLContext();
            var transformer = context.Model.Load(_storagePath, out _);

            if (!File.Exists(_storagePath)) return null;

            var jsonTest = File.ReadAllText(_optionsStoragePath);
            var options = JsonSerializer.Deserialize<TrainingDataOptions>(jsonTest);
            if (options is null) return null;

            return new ModelData() { Transformer = transformer, Options = options };
        }
        catch
        {
            return null;
        }
    }
    public void Save(ModelData modelData, DataViewSchema inputSchema)
    {
        if (!Directory.Exists(Path.GetDirectoryName(_storagePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_storagePath));

        if (!Directory.Exists(Path.GetDirectoryName(_optionsStoragePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_optionsStoragePath));

        var context = new MLContext();
        context.Model.Save(modelData.Transformer, inputSchema, _storagePath);

        var jsonText = JsonSerializer.Serialize(modelData.Options, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(_optionsStoragePath, jsonText);
    }
}
