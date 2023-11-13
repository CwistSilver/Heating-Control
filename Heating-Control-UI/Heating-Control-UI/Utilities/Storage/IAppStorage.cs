using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities.Storage;
/// <summary>
/// Defines methods for persistently storing and retrieving application data, with support for unique value association per class.
/// </summary>
public interface IAppStorage
{
    /// <summary>
    /// Adds or updates a value in the storage, identified by a key. The value can be uniquely associated with the calling class if specified.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <param name="key">The key for the value. If not provided, the caller member name is used as the key.</param>
    /// <param name="uniquePerClass">If true, associates the value uniquely with the calling class, allowing separate values for the same key across different classes.</param>
    public void AddOrSet(object value, [CallerMemberName] string key = "", bool uniquePerClass = false);

    /// <summary>
    /// Retrieves a value from the storage based on the provided key, with an option for class-specific retrieval.
    /// </summary>
    /// <typeparam name="T">The type of the value to be retrieved.</typeparam>
    /// <param name="key">The key for the value. If not provided, the caller member name is used as the key.</param>
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
    /// <param name="uniquePerClass">If true, retrieves a value uniquely associated with the calling class for the given key.</param>
    /// <returns>The retrieved <typeparamref name="T"/> or the <paramref name="defaultValue"/> value if the key is not found.</returns>
    public T? Get<T>([CallerMemberName] string key = "", T? defaultValue = default, bool uniquePerClass = false);

    /// <summary>
    /// Asynchronously saves all current data in the storage, ensuring persistence across app sessions.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task Save();
}
