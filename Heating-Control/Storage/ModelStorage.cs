using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using Tensorflow.Keras.Engine;
using static Tensorflow.KerasApi;

namespace Heating_Control.Storage;
public sealed class ModelStorage : IModelStorage
{
    private const string FileName = "KerasModel.h5";
    private const string OptionsFileName = "HeatingControl-Options.json";
    private readonly string _storagePath;
    private readonly string _directoryPath;
    private readonly ILogger<IModelStorage> _logger;

    public ModelStorage(ILogger<IModelStorage> logger)
    {
        _logger = logger;

        _directoryPath = GetDataDirectorPath();
        _storagePath = Path.Combine(_directoryPath, FileName);

        _logger.LogInformation("ModelStorage initialized. Model path: {0}", _storagePath);
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

    public IModel? Load()
    {
        try
        {
            if (!ExistsModel())
            {
                CopyModelFromResources();
            }

            _logger.LogInformation("Loading model from {0}", _storagePath);
            return LoadModel() ?? throw new FileNotFoundException("Model is missing.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while loading the model");
            return null;
        }
    }

    private bool ExistsModel() => Directory.Exists(_storagePath);
    private void CopyModelFromResources()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            var manifestResourceName = names.FirstOrDefault(file => file.Contains(FileName));
            if (string.IsNullOrEmpty(manifestResourceName)) return;


            var zipPath = Path.Combine(_directoryPath, $"{FileName}.zip");
            var unzipPath = Path.Combine(_directoryPath, "temp");

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if (Directory.Exists(unzipPath))
            {
                Directory.Delete(unzipPath, true);
            }

            if (Directory.Exists(_storagePath))
            {
                Directory.Delete(_storagePath, true);
            }

            using (var resourceStream = assembly.GetManifestResourceStream(manifestResourceName))
            using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            {
                resourceStream.CopyTo(fileStream);
            }

            if (File.Exists(zipPath))
            {
                ZipFile.ExtractToDirectory(zipPath, unzipPath);
                File.Delete(zipPath);

               

                var tempFolderContentPath = Path.Combine(_directoryPath, "temp", FileName);
                var files = Directory.GetFiles(unzipPath);
                var directories = Directory.GetDirectories(unzipPath);
                var files2 = Directory.GetFiles(tempFolderContentPath);

                Directory.Move(tempFolderContentPath, _storagePath);
                Directory.Delete(unzipPath);
            }

            int i = 0;
        }
        catch (Exception ex)
        {
            int i = 0;
        }

    }

    private IModel? LoadModel()
    {
        try
        {
            var model = keras.models.load_model(_storagePath);
            return model;
        }
        catch(Exception ex) 
        {
            return null;
        }
    }
}
