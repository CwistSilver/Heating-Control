using Tensorflow;

namespace Heating_Control.Data;
public sealed class NeuralNetworkModel
{
    public Tensor X { get; init; }
    public Tensor Y { get; init; }
    public ResourceVariable[] Variables { get; init; }
    public Tensor Prediction { get; init; }
    public Tensor Cost { get; init; }
    public Operation Optimizer { get; init; }
}

