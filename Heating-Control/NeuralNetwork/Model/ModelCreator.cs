using Heating_Control.Data;
using Tensorflow;
using static Tensorflow.Binding;

namespace Heating_Control.NeuralNetwork.Model;
public sealed class ModelCreator : IModelCreator
{
    private const float learning_rate = 0.001f;
    private const int inputSize = 3;
    private const int outputSize = 1;
    private readonly int[] hiddenLayers = new int[3] { 15, 30, 15 };

    public ModelCreator()
    {
        tf.compat.v1.disable_eager_execution();
    }

    public NeuralNetworkModel? _currentModel = null;
    
    public NeuralNetworkModel CurrentModel {
        get
        {
            if(_currentModel is not null)
                return _currentModel;

            _currentModel = CreateModel();
            return _currentModel;
        }
    }

    private NeuralNetworkModel CreateModel()
    {
        var X = tf.placeholder(tf.float32);
        var Y = tf.placeholder(tf.float32);

        Tensor lastLayerOutput = X;
        var variables = new List<ResourceVariable>();

        for (int i = 0; i < hiddenLayers.Length; i++)
        {
            int layerInputSize = i == 0 ? inputSize : hiddenLayers[i - 1];
            var W = tf.Variable(tf.random_uniform(new Shape(layerInputSize, hiddenLayers[i]), -1.0f, 1.0f));
            var b = tf.Variable(tf.zeros(new Shape(hiddenLayers[i])));

            lastLayerOutput = tf.nn.sigmoid(tf.matmul(lastLayerOutput, W) + b);
            //lastLayerOutput = tf.nn.relu(tf.matmul(lastLayerOutput, W) + b);

            variables.Add(W);
            variables.Add(b);
        }

        var W_output = tf.Variable(tf.random_uniform(new Shape(hiddenLayers.Last(), outputSize), -1.0f, 1.0f));
        var b_output = tf.Variable(tf.zeros(new Shape(outputSize)));

        var pred = tf.matmul(lastLayerOutput, W_output) + b_output;

        variables.Add(W_output);
        variables.Add(b_output);

        var cost = tf.reduce_mean(tf.square(pred - Y));
        var optimizer = tf.train.AdamOptimizer(learning_rate).minimize(cost);


        return new NeuralNetworkModel()
        {
            X = X,
            Y = Y,
            Variables = variables.ToArray(),
            Prediction = pred,
            Cost = cost,
            Optimizer = optimizer
        };
    }
}
