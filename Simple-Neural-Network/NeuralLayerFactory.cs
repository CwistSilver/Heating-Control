using Simple_Neural_Network.Activation;
using Simple_Neural_Network.Input;
using Simple_Neural_Network.Layers;
using Simple_Neural_Network.Neurons;

namespace Simple_Neural_Network;
public class NeuralLayerFactory
{
    public NeuralLayer CreateNeuralLayer(int numberOfNeurons, IActivationFunction activationFunction, IInputFunction inputFunction)
    {
        var layer = new NeuralLayer();

        for (int i = 0; i < numberOfNeurons; i++)
        {
            var neuron = new Neuron(activationFunction, inputFunction);
            layer.Neurons.Add(neuron);
        }

        return layer;
    }
}
