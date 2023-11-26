namespace Simple_Neural_Network.Activation;
/// <summary>
/// Interface for activation functions.
/// </summary>
public interface IActivationFunction
{
    double CalculateOutput(double input);
}
