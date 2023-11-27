using Heating_Control.Data;
using Microsoft.ML;
using Tensorflow;

namespace Heating_Control.ML.Storage;
/// <summary>
/// Defines methods for loading and saving model data.
/// </summary>
public interface IModelStorage
{
    /// <summary>
    /// Loads the model data from storage.
    /// </summary>
    /// <returns>A <see cref="ModelData"/> object if successful, null otherwise.</returns>
    ModelData? Load(Session session);

    /// <summary>
    /// Saves the provided model data and input schema to storage.
    /// </summary>
    /// <param name="modelData">The model data to be saved.</param>
    /// <param name="inputSchema">The schema of the input data used for the model.</param>
    void Save(ModelData modelData);
}