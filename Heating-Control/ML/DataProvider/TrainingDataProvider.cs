using Heating_Control.Data;

namespace Heating_Control.ML.DataProvider;
public sealed class TrainingDataProvider : ITrainingDataProvider
{
    private readonly Random _random;
    public TrainingDataProvider()
    {
        _random = new Random();
    }

    public async Task<IReadOnlyList<HeatingControlTrainingData>> GenerateAsync(TrainingDataOptions options)
    {
        return await Task.Run(() =>
        {
            var trainingDatas = new HeatingControlTrainingData[options.RecordsToGenerate];
            for (int i = 0; i < trainingDatas.Length; i++)
            {
                trainingDatas[i] = GenerateRandom(options);
            }
            return trainingDatas as IReadOnlyList<HeatingControlTrainingData>;
        });
    }

    private HeatingControlTrainingData GenerateRandom(TrainingDataOptions options)
    {
        var data = new HeatingControlTrainingData();
        data.OutdoorTemperature = GenerateRandomTemperature(options.OutdoorTemperatureRange);
        data.PreferredIndoorTemperature = GenerateRandomTemperature(options.PreferredIndoorTemperatureRange);
        data.PredictedOutdoorTemperature = GeneratePredictedOutdoorTemperature(options, data);
        data.SupplyTemperature = GenerateSupplyTemperature(options, data);
        return data;
    }

    private float GenerateRandomTemperature(TemperatureRange temperatureRange)
    {
        var difference = temperatureRange.Max - temperatureRange.Min;
        return temperatureRange.Min + (float)_random.NextDouble() * difference;
    }

    private float GeneratePredictedOutdoorTemperature(TrainingDataOptions options, HeatingControlTrainingData data)
    {
        var predictedOutdoorOffset = GenerateRandomTemperature(options.PredictedOutdoorTemperatureOffsetRange);
        var outdoorTemperature = data.OutdoorTemperature + predictedOutdoorOffset;
        return Math.Clamp(outdoorTemperature, options.OutdoorTemperatureRange.Min, options.OutdoorTemperatureRange.Max);
    }

    // Quelle:https://www.viessmann-community.com/t5/Gas/Mathematische-Formel-fuer-Vorlauftemperatur-aus-den-vier/m-p/74706#M27562
    private float GenerateSupplyTemperature(TrainingDataOptions options, HeatingControlTrainingData data)
    {
        var inaccuracyPercent = (float)_random.NextDouble() * options.SupplyTemperatureInaccuracy / 100f + 0.8f;
        var insideOutsideTemperatureDifference = Math.Abs(data.PredictedOutdoorTemperature - data.OutdoorTemperature);
        var insideOutsideTemperature = Math.Max(data.PredictedOutdoorTemperature, data.OutdoorTemperature) - insideOutsideTemperatureDifference * inaccuracyPercent;


        var rtSoll = data.PreferredIndoorTemperature;
        var dar = insideOutsideTemperature - data.PreferredIndoorTemperature;
        if (insideOutsideTemperature >= data.PreferredIndoorTemperature)
            return 0;

        var vtSoll = rtSoll + options.Baseline - options.Gradient * dar * (1.4347f + 0.021f * dar + 247.9f * Math.Pow(10, -6) * Math.Pow(dar, 2));
        return Math.Clamp((float)vtSoll, insideOutsideTemperature, options.MaxSupplyTemperature);
    }
}
