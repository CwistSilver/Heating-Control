using Microsoft.ML;

namespace Heating_Control.Data;
public class ModelData
{
    public required ITransformer Transformer { get; set; }
    public required TrainingDataOptions Options { get; set; }
}
