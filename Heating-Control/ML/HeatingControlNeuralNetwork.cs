using Heating_Control.Data;
using Heating_Control.ML.Storage;
using Heating_Control.ML.Trainer;
using Heating_Control.NeuralNetwork.Model;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Tensorflow;
using static Tensorflow.Binding;

namespace Heating_Control.ML;
public sealed class HeatingControlNeuralNetwork : IHeatingControlNeuralNetwork
{
    public TrainingDataOptions? UsedTrainingDataOptions { get; private set; }

    //private ITransformer? _transformer;
    //private PredictionEngine<HeatingControlInputData, HeatingControlPrediction>? _predictionEngine;
    private Session? _session;
    private NeuralNetworkModel _model;

    private readonly IHeatingControlTrainer _heatingControlTrainer;
    private readonly IModelStorage _modelStorage;
    private readonly ILogger<IHeatingControlNeuralNetwork> _logger;
    private readonly IModelCreator _modelCreator;

    public HeatingControlNeuralNetwork(IHeatingControlTrainer heatingControlTrainer, IModelStorage modelStorage, IModelCreator modelCreator, ILogger<IHeatingControlNeuralNetwork> logger)
    {
        _heatingControlTrainer = heatingControlTrainer;
        _modelStorage = modelStorage;
        _modelCreator = modelCreator;
        _logger = logger;


        tf.compat.v1.disable_eager_execution();
    }

    //public HeatingControlPrediction Predict(HeatingControlInputData inputData)
    //{
    //    if (_predictionEngine is null)
    //        throw new InvalidOperationException("Neural network has not been initialized yet");

    //    var predictionResult = _predictionEngine.Predict(inputData);
    //    return predictionResult;
    //}

    public HeatingControlPrediction Predict(HeatingControlInputData input)
    {
        float[,] input_X = new float[1, 3];
        input_X[0, 0] = input.OutdoorTemperature;
        input_X[0, 1] = input.PredictedOutdoorTemperature;
        input_X[0, 2] = input.PreferredIndoorTemperature;

        var predictions = _session.run(_model.Prediction, new FeedItem(_model.X, input_X));
        return new HeatingControlPrediction() { SupplyTemperature = predictions[0, 0] * 90f };
    }

    public void Inizialize()
    {
        if (_session is not null)
        {
            _logger.LogError("Attempted to initialize the neural network, but it has already been initialized.");
            throw new InvalidOperationException("Neural network has already been initialized");
        }

        _session = tf.compat.v1.Session();
        //_modelStorage.Load()
        _model = _modelCreator.CurrentModel;
        var saver = tf.train.Saver();
        
        //_session.run(tf.compat.v1.global_variables_initializer());

        var modelData = _modelStorage.Load(_session, saver) ?? throw new FileLoadException("Neural network could not be loaded");
        //_transformer = modelData.Data;
        UsedTrainingDataOptions = modelData.Options;

        //_predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
        _logger.LogInformation("Neural network initialized successfully.");
    }

    public async Task TrainModel(TrainingDataOptions? options = null)
    {
        _logger.LogInformation("Starting to train the neural network model.");

       

        var newSession = tf.compat.v1.Session();
        await _heatingControlTrainer.TrainNeuralNetworkAsync(newSession, options);
        //_predictionEngine = new MLContext().Model.CreatePredictionEngine<HeatingControlInputData, HeatingControlPrediction>(_transformer);
        //UsedTrainingDataOptions = options is null ? new TrainingDataOptions() : options;

        _logger.LogInformation("Model training completed.");

        var oldSession = _session;
        _session = newSession;

        if (oldSession is not null)
        {
            oldSession.Dispose();
        }

    }
}