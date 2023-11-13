using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace Heating_Control_UI.Utilities;
/// <summary>
/// Provides a global cache for managing and optimizing the loading of resources such as images and streams.
/// </summary>
internal static class GlobalCache
{
    private static readonly ConcurrentDictionary<string, object> _cache = new();
    private static string? currentAssemblyName;

    private static void AddToCache(string path, object obj) => _cache.TryAdd(path, obj);
    private static object? GetFromCache(string path)
    {
        if (_cache.TryGetValue(path, out var obj))
            return obj;

        return null;
    }

    /// <summary>
    /// Retrieves an image from the cache, or loads and caches it if not already cached.
    /// </summary>
    /// <param name="path">The path or URI of the image.</param>
    /// <param name="doCache">Indicates whether to cache the image after loading.</param>
    /// <returns>The requested image, or null if the path is invalid.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provided path format is not supported.</exception>
    public static IImage? GetImage(string path, bool doCache = true)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (path is string rawUri)
        {
            Uri uri;

            if (rawUri.StartsWith("avares://"))
            {
                uri = new Uri(rawUri);
            }
            else
            {
                if (currentAssemblyName is null)
                    currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;


                uri = new Uri($"avares://{currentAssemblyName}/{rawUri}");
            }


            var cachedObj = GetFromCache(uri.ToString());
            if (cachedObj is not null)
            {
                return (Bitmap)cachedObj;
            }

            var asset = AssetLoader.Open(uri);

            var bitmap = new Bitmap(asset);
            if (doCache)
            {
                AddToCache(uri.ToString(), bitmap);
            }

            return bitmap;
        }

        throw new NotSupportedException();
    }

    /// <summary>
    /// Retrieves a stream from the cache, or loads and caches it if not already cached.
    /// </summary>
    /// <param name="path">The path or URI of the resource.</param>
    /// <param name="doCache">Indicates whether to cache the stream after loading.</param>
    /// <returns>The requested stream, or null if the path is invalid.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provided path format is not supported.</exception>
    public static Stream? GetStream(string path, bool doCache = true)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        if (path is string rawUri)
        {
            Uri uri;

            if (rawUri.StartsWith('/'))
            {
                rawUri = rawUri.Substring(1);
            }

            if (rawUri.StartsWith("avares://"))
            {
                uri = new Uri(rawUri);
            }
            else
            {
                if (currentAssemblyName is null)
                    currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                uri = new Uri($"avares://{currentAssemblyName}/{rawUri}");
            }

            var cachedObj = GetFromCache(uri.ToString());
            if (cachedObj is not null)
            {
                return new MemoryStream((byte[])cachedObj);
            }

            var asset = AssetLoader.Open(uri);

            byte[] data;
            using (var ms = new MemoryStream())
            {
                asset.CopyTo(ms);
                data = ms.ToArray();
            }

            if (doCache)
            {
                AddToCache(uri.ToString(), data);
            }

            return new MemoryStream(data);
        }

        throw new NotSupportedException();
    }
}