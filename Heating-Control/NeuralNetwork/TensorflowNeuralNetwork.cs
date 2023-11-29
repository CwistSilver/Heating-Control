using Heating_Control.Data;
using Heating_Control.ML.DataProvider;
using System.Runtime.InteropServices;
using Tensorflow;
using static Tensorflow.Binding;

namespace Heating_Control.NeuralNetwork;
public class TensorflowNeuralNetwork
{
    private const float learning_rate = 0.01f;
    private const uint batchSize = 1_000;
    private const uint training_epochs = 10_000;
    private readonly int[] hiddenLayers = new int[3] { 5, 10, 5 };

    private const string FileName = "HeatingControl.ckpt";
    private readonly string _storagePath;
    private readonly Session _session;
    private readonly NeuralNetworkModel _model;
    private readonly Saver _saver;

    public TensorflowNeuralNetwork()
    {
        var directory = GetDataDirectorPath();
        _storagePath = Path.Combine(directory, FileName);
        tf.compat.v1.disable_eager_execution();
        _session = tf.compat.v1.Session();

        _model = CreateModel(3, 1, hiddenLayers);

        _saver = tf.train.Saver();
    }

    public HeatingControlPrediction Predict(HeatingControlInputData input)
    {
        float[,] input_X = new float[1, 3];
        input_X[0, 0] = input.OutdoorTemperature;
        input_X[0, 1] = input.PredictedOutdoorTemperature;
        input_X[0, 2] = input.PreferredIndoorTemperature;

        var predictions = _session.run(_model.Prediction, new FeedItem(_model.X, input_X));
        return new HeatingControlPrediction() { SupplyTemperature = predictions[0, 0] * 90f };
    }

    public void Save()
    {
        _saver.save(_session, _storagePath);
    }

    public void Load()
    {
        _saver.restore(_session, _storagePath);
    }

    private static void Train(IReadOnlyList<HeatingControlTrainingData> data, IReadOnlyList<HeatingControlTrainingData> testDatas)
    {
        int n_samples = data.Count;

        // Eingabe- und Ziel-Daten vorbereiten
        float[,] train_X = new float[n_samples, 3];
        float[,] train_Y = new float[n_samples, 1];

        for (int i = 0; i < n_samples; i++)
        {
            train_X[i, 0] = data[i].OutdoorTemperature;
            train_X[i, 1] = data[i].PredictedOutdoorTemperature;
            train_X[i, 2] = data[i].PreferredIndoorTemperature;
            train_Y[i, 0] = data[i].SupplyTemperature;
        }

        tf.compat.v1.disable_eager_execution();

        // Modellparameter
        var learning_rate = 0.0001f; // Versuchen Sie einen kleineren Wert

        var training_epochs = 1000;
        var display_step = 50;
        int n_hidden = 1;

        // Tensorflow-Platzhalter für Eingabe- und Ziel-Daten
        var X = tf.placeholder(tf.float32);
        var Y = tf.placeholder(tf.float32);


        // Gewichte und Bias für die verborgene Schicht
        var W_hidden = tf.Variable(tf.random_uniform(new Shape(3, n_hidden), -1.0f, 1.0f));
        var b_hidden = tf.Variable(tf.zeros(new Shape(n_hidden)));

        // Ausgabe der verborgenen Schicht
        var hidden_output = tf.nn.relu(tf.matmul(X, W_hidden) + b_hidden);

        // Gewichte und Bias für die Ausgabeschicht
        var W_output = tf.Variable(tf.random_uniform(new Shape(n_hidden, 1), -1.0f, 1.0f));
        var b_output = tf.Variable(tf.zeros(1));

        // Vorhersagemodell
        var pred = tf.matmul(hidden_output, W_output) + b_output;

        // Kostenfunktion und Optimierer
        var cost = tf.reduce_mean(tf.square(pred - Y));
        var optimizer = tf.train.GradientDescentOptimizer(learning_rate).minimize(cost);

        // TensorFlow-Sitzung initialisieren
        using (var sess = tf.compat.v1.Session())
        {
            // Variablen initialisieren
            sess.run(tf.compat.v1.global_variables_initializer());

            // Trainingszyklus
            for (int epoch = 0; epoch < training_epochs; epoch++)
            {
                sess.run(optimizer, new FeedItem(X, train_X), new FeedItem(Y, train_Y));

                if ((epoch + 1) % display_step == 0)
                {
                    var c = sess.run(cost, new FeedItem(X, train_X), new FeedItem(Y, train_Y));
                    Console.WriteLine($"Epoch: {epoch + 1}, cost={c}, W_hidden={sess.run(W_hidden)}, b_hidden={sess.run(b_hidden)}, W_output={sess.run(W_output)}, b_output={sess.run(b_output)}");
                }
            }

            Console.WriteLine("Optimierung fertig!");

            foreach (var testData in testDatas)
            {
                // Vorhersage mit Testdaten
                float[,] input_X = new float[1, 3];
                input_X[0, 0] = testData.OutdoorTemperature;
                input_X[0, 1] = testData.PredictedOutdoorTemperature;
                input_X[0, 2] = testData.PreferredIndoorTemperature;

                var predicted_Y = sess.run(pred, new FeedItem(X, input_X));
                Console.WriteLine($"Vorhergesagte Vorlauftemperatur für Testdaten: {predicted_Y[0, 0]}");
                Console.WriteLine($"Tatsächliche Vorlauftemperatur der Testdaten: {testData.SupplyTemperature}");
            }
        }
    }

    public async Task<NeuralNetworkModel> Train(ITrainingDataProvider trainingDataProvider)
    {
        _session.run(tf.compat.v1.global_variables_initializer());

        for (int epoch = 0; epoch < training_epochs; epoch++)
        {
            var data = await trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = batchSize });

            // Eingabe- und Ziel-Daten vorbereiten
            float[,] train_X = new float[batchSize, 3];
            float[,] train_Y = new float[batchSize, 1];

            for (int i = 0; i < batchSize; i++)
            {
                train_X[i, 0] = data[i].OutdoorTemperature;
                train_X[i, 1] = data[i].PredictedOutdoorTemperature;
                train_X[i, 2] = data[i].PreferredIndoorTemperature;


                train_Y[i, 0] = data[i].SupplyTemperature / 90f;
            }


            _session.run(_model.Optimizer, new FeedItem(_model.X, train_X), new FeedItem(_model.Y, train_Y));
     
        }

        return _model;
    }

    private static NeuralNetworkModel CreateModel(int inputSize, int outputSize, int[] hiddenLayers)
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

    private static string GetDataDirectorPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), nameof(Heating_Control));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }
}
