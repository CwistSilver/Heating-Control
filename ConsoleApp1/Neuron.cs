namespace ConsoleApp1;
public class Neuron
{
    public double[] Weights;
    public double Bias;

    public Neuron(int inputSize)
    {
        Weights = new double[inputSize];
        Random rand = new Random();
        for (int i = 0; i < inputSize; i++)
        {
            Weights[i] = rand.NextDouble() * 2 - 1; // Zufällige Initialisierung
        }
        Bias = rand.NextDouble() * 2 - 1;
    }

    public double ProcessInputs(double[] inputs)
    {
        double output = 0.0;
        for (int i = 0; i < inputs.Length; i++)
        {
            output += inputs[i] * Weights[i];
        }
        output += Bias;

        return Sigmoid(output);
    }

    private double Sigmoid(double input)
    {
        return 1.0 / (1 + Math.Exp(-input));
    }
}
