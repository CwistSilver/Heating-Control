using Microsoft.ML;

namespace Heating_Control.Data;
/// <summary>
/// Represents the data model containing the transformer and training options for the neural network.
/// </summary>
public class ModelData
{
    /// <summary>
    /// The transformer used in the neural network model.
    /// </summary>
    public required ITransformer Transformer { get; set; }

    /// <summary>
    /// The training data options for the neural network model.
    /// </summary>
    public required TrainingDataOptions Options { get; set; }
}
