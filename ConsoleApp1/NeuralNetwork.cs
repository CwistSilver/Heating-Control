using Simple_Neural_Network.Neurons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1;
public class NeuralNetwork
{
    private Neuron[] hiddenLayer;
    private Neuron outputNeuron;

    public NeuralNetwork(int inputSize, int hiddenLayerSize)
    {
        hiddenLayer = new Neuron[hiddenLayerSize];
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            hiddenLayer[i] = new Neuron(inputSize);
        }
        outputNeuron = new Neuron(hiddenLayerSize);
    }

    public double ProcessInput(double[] inputs)
    {
        double[] hiddenOutputs = new double[hiddenLayer.Length];
        for (int i = 0; i < hiddenLayer.Length; i++)
        {
            hiddenOutputs[i] = hiddenLayer[i].ProcessInputs(inputs);
        }
        return outputNeuron.ProcessInputs(hiddenOutputs);
    }
}
