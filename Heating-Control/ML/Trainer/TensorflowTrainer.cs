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
    private const uint training_epochs = 1_000;
    private readonly int[] hiddenLayers = new int[3] { 5, 10, 5 };

    private readonly Session _session;
    private readonly NeuralNetworkModel _model;

    private readonly ITrainingDataProvider _trainingDataProvider;
    private readonly IModelStorage _modelStorage;
    private readonly IModelCreator _modelCreator;
    private readonly ILogger<IHeatingControlTrainer> _logger;
    public TensorflowTrainer(ITrainingDataProvider trainingDataProvider, IModelStorage modelStorage, IModelCreator modelCreator, ILogger<IHeatingControlTrainer> logger)
    {
        _trainingDataProvider = trainingDataProvider;
        _modelStorage = modelStorage;
        _modelCreator = modelCreator;
        _logger = logger;

        tf.compat.v1.disable_eager_execution();
        _session = tf.compat.v1.Session();
        _model = _modelCreator.CreateModel();

        _session.run(tf.compat.v1.global_variables_initializer());
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

    public void Load()
    {       
        var model= _modelStorage.Load(_session);
        //_session = model.Data;

       
    }

    public async Task<NeuralNetworkModel> TrainNeuralNetworkAsync(TrainingDataOptions? options = null)
    {       
        options ??= new TrainingDataOptions();
        options.RecordsToGenerate = batchSize;

        _logger.LogInformation("Starting training process.");
        var stopwatch = Stopwatch.StartNew();
        //_session.run(tf.compat.v1.global_variables_initializer());

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


            _session.run(_model.Optimizer, new FeedItem(_model.X, train_X), new FeedItem(_model.Y, train_Y));
        }

        stopwatch.Stop();
        _logger.LogInformation("Training completed. Duration: {Duration} ms", stopwatch.ElapsedMilliseconds);
      
        _modelStorage.Save(new ModelData() { Data = _session, Options = options });
        _logger.LogInformation("Model saved successfully.");

        return _model;
    }
}
