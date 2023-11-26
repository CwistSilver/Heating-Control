namespace Simple_Neural_Network.Activation;
/// <summary>
/// Implementation of Rectifier Activation Function.
/// </summary>
public class RectifiedActivationFuncion : IActivationFunction
{
    public double CalculateOutput(double input)
    {
        return Math.Max(0, input);
    }
}
