using Tensorflow.Keras.Engine;

namespace Heating_Control.Storage;
/// <summary>
/// Defines methods for loading and saving model data.
/// </summary>
public interface IModelStorage
{
    /// <summary>
    /// Loads neural network model from storage.
    /// </summary>
    /// <returns>A <see cref="IModel"/> object if successful, null otherwise.</returns>
    IModel? Load();
}