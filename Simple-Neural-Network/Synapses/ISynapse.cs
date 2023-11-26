namespace Simple_Neural_Network.Synapses;
/// <summary>
/// Interface for synapses (connections).
/// </summary>
public interface ISynapse
{
    double Weight { get; set; }
    double PreviousWeight { get; set; }
    double GetOutput();

    bool IsFromNeuron(Guid fromNeuronId);
    void UpdateWeight(double learningRate, double delta);
}
