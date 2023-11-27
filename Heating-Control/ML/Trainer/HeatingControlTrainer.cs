//using Heating_Control.Data;
//using Heating_Control.ML.DataProvider;
//using Heating_Control.ML.Storage;
//using Microsoft.Extensions.Logging;
//using Microsoft.ML;
//using Microsoft.ML.Data;
//using Microsoft.ML.Trainers.FastTree;
//using System.Diagnostics;

//namespace Heating_Control.ML.Trainer;
//public sealed class HeatingControlTrainer : IHeatingControlTrainer
//{
//    private readonly ITrainingDataProvider _trainingDataProvider;
//    private readonly IModelStorage _modelStorage;
//    private readonly ILogger<IHeatingControlTrainer> _logger;
//    public HeatingControlTrainer(ITrainingDataProvider trainingDataProvider, IModelStorage modelStorage, ILogger<IHeatingControlTrainer> logger)
//    {
//        _trainingDataProvider = trainingDataProvider;
//        _modelStorage = modelStorage;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Creates and trains a new neural network asynchronously.
//    /// <para>
//    /// The
//    /// <a href="https://learn.microsoft.com/de-de/dotnet/api/microsoft.ml.trainers.fasttree.fasttreetweedietrainer?view=ml-dotnet" target="_blank">
//    /// FastTreeTweedieTrainer
//    /// </a>is used for this.<br/>
//    /// </para>
//    /// </summary>
//    /// <param name="options">The options with which the neural network is to be trained.</param>
//    /// <returns>The transformer of the NeuralNetwork</returns>
//    public async Task<ITransformer> TrainNeuralNetworkAsync(TrainingDataOptions? options = null)
//    {
         
//        options ??= new TrainingDataOptions() { RecordsToGenerate = 1_000_000 };
//        var trainingData = await _trainingDataProvider.GenerateAsync(options);

//        var context = new MLContext();
//        var dataView = context.Data.LoadFromEnumerable(trainingData);
//        var dataViewSchema = dataView.Schema;

//        _logger.LogInformation("Starting training process.");
//        var stopwatch = Stopwatch.StartNew();
//        var model = await Task.Run(() =>
//        {
//            var pipeline = CreateNeuralNetworkPipeline(context);
//            var trainedModel = pipeline.Fit(dataView);

//            return trainedModel;
//        });

//        stopwatch.Stop();
//        _logger.LogInformation("Training completed. Duration: {Duration} ms", stopwatch.ElapsedMilliseconds);

//        _modelStorage.Save(new ModelData() { Data = model, Options = options }, dataView.Schema);
//        _logger.LogInformation("Model saved successfully.");

//        return model;
//    }

//    private static IEstimator<ITransformer> CreateNeuralNetworkPipeline(MLContext context)
//    {
//        var pipeline = context.Transforms
//            .Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
//            .Append(context.Transforms.NormalizeMinMax("Features"))
//   .Append(context.Regression.Trainers.OnlineGradientDescent(labelColumnName: "Label", featureColumnName: "Features"));

//        return pipeline;
//    }
//}