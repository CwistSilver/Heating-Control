using Heating_Control.Data;
using Heating_Control.ML.DataProvider;
using Heating_Control.ML.Storage;
using Heating_Control.NeuralNetwork.Model;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Tensorflow;
using static Tensorflow.Binding;

namespace Heating_Control.ML.Trainer;
public sealed class TensorflowTrainer : IHeatingControlTrainer
{
    private const uint batchSize = 1_000;
    private const uint training_epochs = 40_000;

    //private readonly Session _session;
    private readonly NeuralNetworkModel _model;

    private readonly ITrainingDataProvider _trainingDataProvider;
    private readonly IModelStorage _modelStorage;
    private readonly ILogger<IHeatingControlTrainer> _logger;
    private readonly Saver _saver;

    public TensorflowTrainer(ITrainingDataProvider trainingDataProvider, IModelStorage modelStorage, IModelCreator modelCreator, ILogger<IHeatingControlTrainer> logger)
    {
        _trainingDataProvider = trainingDataProvider;
        _modelStorage = modelStorage;
        _logger = logger;

        tf.compat.v1.disable_eager_execution();
        //_session = tf.compat.v1.Session();

        _model = modelCreator.CurrentModel;
        _saver = tf.train.Saver();
    }


    //public HeatingControlPrediction Predict(HeatingControlInputData input)
    //{
    //    float[,] input_X = new float[1, 3];
    //    input_X[0, 0] = input.OutdoorTemperature;
    //    input_X[0, 1] = input.PredictedOutdoorTemperature;
    //    input_X[0, 2] = input.PreferredIndoorTemperature;

    //    var predictions = _session.run(_model.Prediction, new FeedItem(_model.X, input_X));
    //    return new HeatingControlPrediction() { SupplyTemperature = predictions[0, 0] };
    //}

    //public void Load()
    //{
    //    var model = _modelStorage.Load(_session);
    //    //_session = model.Data;
    //    //_session.run(tf.compat.v1.global_variables_initializer());
    //    //_saver.restore(_session, _storagePath);

    //}

    public async Task<NeuralNetworkModel> TrainNeuralNetworkAsync(Session session, TrainingDataOptions? options = null)
    {
        options ??= new TrainingDataOptions();
        options.RecordsToGenerate = batchSize;

        _logger.LogInformation("Starting training process.");
        var stopwatch = Stopwatch.StartNew();
        session.run(tf.compat.v1.global_variables_initializer());

        var display_step = 1000;
        double lossAvarage = 0;
        double accuracyAvarage = 0;
        var lossList = new List<float>();
        var accuracyList = new List<float>();

        for (int epoch = 0; epoch < training_epochs; epoch++)
        {
            var data = await _trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = batchSize });

            float[,] train_X = new float[batchSize, 3];
            float[,] train_Y = new float[batchSize, 1];

            for (int i = 0; i < batchSize; i++)
            {
                train_X[i, 0] = data[i].OutdoorTemperature;
                train_X[i, 1] = data[i].PredictedOutdoorTemperature;
                train_X[i, 2] = data[i].PreferredIndoorTemperature;


                train_Y[i, 0] = data[i].SupplyTemperature / 90f;
            }

            session.run(_model.Optimizer, new FeedItem(_model.X, train_X), new FeedItem(_model.Y, train_Y));

            if ((epoch + 1) % display_step == 0)
            {
                var loss = session.run(_model.Cost, new FeedItem(_model.X, train_X), new FeedItem(_model.Y, train_Y));
                float lossValue = loss[0];

                //var accuracy = await CalcAccuracy();

                lossAvarage += lossValue;
                //accuracyAvarage += accuracy;
                lossList.Add(lossValue);
                //accuracyList.Add(accuracy);
                Console.WriteLine($"Epoche: {epoch + 1}, Loss: {lossValue.ToString("F6")}");
                //Console.WriteLine($"Epoche: {epoch + 1}, Loss: {lossValue.ToString("F6")}, Accuracy: {accuracy.ToString("F6")}");
                // Für die Accuracy-Berechnung müssen Sie eine geeignete Logik hinzufügen
                // Beispiel: var accuracy = CalculateAccuracy(_model.Prediction, train_Y);
                // Console.WriteLine($"Epoche: {epoch+1}, Accuracy: {accuracy}");
            }


        }

        stopwatch.Stop();
        _logger.LogInformation("Training completed. Duration: {Duration} ms", stopwatch.ElapsedMilliseconds);

        lossAvarage /= (training_epochs / display_step);
        accuracyAvarage /= (training_epochs / display_step);

        //var lossMedian = Median(lossList);
        //var accuracyMedian = Median(lossList);
        //Console.WriteLine($"Loss Median: {lossMedian.ToString("F6")}, Accuracy Median: {accuracyMedian.ToString("F6")}");
        //Console.WriteLine($"Loss Avarage: {lossAvarage.ToString("F6")}, Accuracy Avarage: {accuracyAvarage.ToString("F6")}");


        _modelStorage.Save(new ModelData() { Data = session, Options = options }, _saver);
        _logger.LogInformation("Model saved successfully.");

        return _model;
    }


    //public static T Median<T>(IEnumerable<T> items)
    //{

    //    var i = (int)Math.Ceiling((double)(items.Count() - 1) / 2);
    //    if (i >= 0)
    //    {
    //        var values = items.ToList();
    //        values.Sort();
    //        return values[i];
    //    }

    //    return default(T);
    //}

    //public async Task<float> CalcAccuracy(int dataSets = 100)
    //{
    //    var testData = await _trainingDataProvider.GenerateAsync(new TrainingDataOptions() { RecordsToGenerate = (uint)dataSets });
    //    List<float[]> differences = new List<float[]>();
    //    foreach (var data in testData)
    //    {
    //        var input = new HeatingControlInputData() { OutdoorTemperature = data.OutdoorTemperature, PredictedOutdoorTemperature = data.PredictedOutdoorTemperature, PreferredIndoorTemperature = data.PreferredIndoorTemperature };

    //        float prediction = Predict(input).SupplyTemperature;
    //        differences.Add([prediction, data.SupplyTemperature]);
    //    }

    //    float accuracy = CalculateAccuracy(differences);
    //    return accuracy;
    //}

    //private float CalculateAccuracy(List<float[]> data)
    //{
    //    if (data is null || data.Count == 0)
    //        throw new ArgumentException("Data list is empty or null.");

    //    float differences = 0;

    //    foreach (float[] entry in data)
    //    {
    //        if (entry.Length != 2)
    //            throw new ArgumentException("Each array in the list should contain exactly two values.");

    //        float predictedValue = entry[0];
    //        float actualValue = entry[1];
    //        float difference = Math.Abs(predictedValue - actualValue);
    //        differences += difference;
    //    }

    //    float averageDifference = differences / data.Count;
    //    return averageDifference;
    //}

}
