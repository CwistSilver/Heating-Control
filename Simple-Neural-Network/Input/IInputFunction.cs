using Simple_Neural_Network.Synapses;

namespace Simple_Neural_Network.Input;
/// <summary>
/// Interface for Input Functions.
/// </summary>
public interface IInputFunction
{
    double CalculateInput(List<ISynapse> inputs);
}
