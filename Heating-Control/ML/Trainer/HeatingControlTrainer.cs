using Heating_Control.Data;
using Heating_Control.ML.DataProvider;
using Heating_Control.ML.Storage;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace Heating_Control.ML.Trainer;
public sealed class HeatingControlTrainer : IHeatingControlTrainer
{
    private readonly ITrainingDataProvider _trainingDataProvider;
    private readonly IModelStorage _modelStorage;
    public HeatingControlTrainer(ITrainingDataProvider trainingDataProvider, IModelStorage modelStorage)
    {
        _trainingDataProvider = trainingDataProvider;
        _modelStorage = modelStorage;
    }

    public async Task<ITransformer> TrainNeuralNetworkAsync(TrainingDataOptions? options = null)
    {
        options ??= new TrainingDataOptions() { RecordsToGenerate = 10_000 };     

        Console.WriteLine($"Create {options.RecordsToGenerate} training datas");
        var startTime = DateTime.Now;
        var trainingData = await _trainingDataProvider.GenerateAsync(options);
        var elapsedTime = DateTime.Now - startTime;
        Console.WriteLine($"Finished to Create training data. Elapsed time: {elapsedTime}");

        var context = new MLContext();
        var dataView = context.Data.LoadFromEnumerable(trainingData);
        var pipeline = CreatePipeline(context);

        Console.WriteLine($"Start Training.");
        var start = DateTime.Now;
        var model = pipeline.Fit(dataView);
        var end = DateTime.Now - start;
        Console.WriteLine($"Finished! Duration: {end}");

        _modelStorage.Save(new ModelData() { Transformer = model, Options = options}, dataView.Schema);

        return model;
    }

    private static EstimatorChain<RegressionPredictionTransformer<LinearRegressionModelParameters>> CreatePipeline(MLContext context)
    {
        var pipeline = context.Transforms
            .Concatenate("Features", "OutdoorTemperature", "PredictedOutdoorTemperature", "PreferredIndoorTemperature")
            .Append(context.Transforms.NormalizeMinMax("Features"))
            .Append(context.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

        return pipeline;
    }
}
