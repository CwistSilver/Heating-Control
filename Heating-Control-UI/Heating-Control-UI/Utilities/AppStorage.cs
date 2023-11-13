using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities;
internal class AppStorage : IAppStorage
{
    private ConcurrentDictionary<string, object> _cachedValues = new();
    private const string FileName = "app-storage.json";
    private readonly string _storagePath;
    private readonly Timer _debounceTimer;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(500);
    public AppStorage()
    {
        var directory = GetDataDirectorPath();
        _storagePath = Path.Combine(directory, FileName);
        _debounceTimer = new Timer(DebounceSave, null, Timeout.Infinite, Timeout.Infinite);
        _ = Load();
    }

    public void AddOrSet(object value, [CallerMemberName] string key = "", bool composeKey = false)
    {
        var compositeKey = key;
        if (composeKey)
        {
            var className = new StackFrame(1).GetMethod().ReflectedType.FullName;
            compositeKey = $"{className}.{key}";
        }

        _cachedValues.AddOrUpdate(compositeKey, value, (currentKey, oldValue) =>
        {
            return value;
        });

        _debounceTimer.Change((int)_debounceTime.TotalMilliseconds, Timeout.Infinite);
    }

    public T? Get<T>([CallerMemberName] string key = "", T? defaultValue = default, bool composeKey = false)
    {
        var compositeKey = key;
        if (composeKey)
        {
            var className = new StackFrame(1).GetMethod().ReflectedType.FullName;
            compositeKey = $"{className}.{key}";
        }

        if (!_cachedValues.TryGetValue(compositeKey, out var value))
            return defaultValue;

        if (value is T variable)
            return variable;

        if (value is JsonElement jsonElement)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
            }
            catch
            {
                return defaultValue;
            }
        }

        try
        {
            return (T?)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private async void DebounceSave(object? state) => await Save();

    public async Task Save()
    {
        var directory = Path.GetDirectoryName(_storagePath);
        if (!Directory.Exists(directory))
            _ = Directory.CreateDirectory(directory!);

        using var fs = new FileStream(_storagePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        await JsonSerializer.SerializeAsync(fs, _cachedValues);
        await fs.FlushAsync();
        fs.Close();

    }

    public async Task Load()
    {
        if (!File.Exists(_storagePath))
            return;

        if (new FileInfo(_storagePath).Length == 0)
            return;

        using var fs = new FileStream(_storagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _cachedValues = await JsonSerializer.DeserializeAsync<ConcurrentDictionary<string,object>>(fs);
        fs.Close();
    }

    private string GetDataDirectorPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control_UI));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control_UI));
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
}
