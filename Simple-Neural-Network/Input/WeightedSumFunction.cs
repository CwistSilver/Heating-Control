using Simple_Neural_Network.Synapses;

namespace Simple_Neural_Network.Input;
/// <summary>
/// Implementation of Weighted Sum Function.
/// </summary>
public class WeightedSumFunction : IInputFunction
{
    public double CalculateInput(List<ISynapse> inputs)
    {
        return inputs.Select(x => x.Weight * x.GetOutput()).Sum();
    }
}