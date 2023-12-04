using Heating_Control.Data.Training;

namespace Heating_Control.Training.DataProvider;
public sealed class TrainingDataProvider : ITrainingDataProvider
{
    private readonly Random _random;
    public TrainingDataProvider()
    {
        _random = new Random();
    }

    public IReadOnlyList<HeatingControlTrainingData> Generate(int recordsToGenerate, bool randomOptions = true)
    {
        var trainingDatas = new HeatingControlTrainingData[recordsToGenerate];
        var trainingDataOption = new TrainingDataOptions();
        for (int i = 0; i < trainingDatas.Length; i++)
        {
            if (randomOptions)
            {
                trainingDataOption.Baseline = (float)_random.NextDouble() * 20f;
                trainingDataOption.Gradient = (float)_random.NextDouble() * 10f;
                trainingDataOption.MaxSupplyTemperature = 70 + (float)(_random.NextDouble() * 30f);
            }

            trainingDatas[i] = GenerateRandom(trainingDataOption);
        }

        return trainingDatas;
    }

    private HeatingControlTrainingData GenerateRandom(TrainingDataOptions options)
    {
        var data = new HeatingControlTrainingData();
        data.OutdoorTemperature = GenerateRandomTemperature(options.OutdoorTemperatureRange);
        data.PreferredIndoorTemperature = GenerateRandomTemperature(options.PreferredIndoorTemperatureRange);
        data.PredictedOutdoorTemperature = GeneratePredictedOutdoorTemperature(options, data);
        data.SupplyTemperature = GenerateSupplyTemperature(options, data);

        data.MaxSupplyTemperature = options.MaxSupplyTemperature;
        data.Baseline = options.Baseline;
        data.Gradient = options.Gradient;

        return data;
    }


    public static List<HeatingControlTrainingData> GetStaticTestSet()
    {
        var inputs = new List<HeatingControlTrainingData>();
        var trainingDataOption = new TrainingDataOptions();

        for (int i = -3; i <= 2; i++)
        {
            var temp = i * 10f;
            var data = new HeatingControlTrainingData();
            data.OutdoorTemperature = temp;
            data.PreferredIndoorTemperature = 15;
            data.PredictedOutdoorTemperature = temp;
            data.SupplyTemperature = GenerateSupplyTemperature(trainingDataOption, data);

            data.MaxSupplyTemperature = trainingDataOption.MaxSupplyTemperature;
            data.Baseline = trainingDataOption.Baseline;
            data.Gradient = trainingDataOption.Gradient;

            inputs.Add(data);
        }

        return inputs;
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
    private static float GenerateSupplyTemperature(TrainingDataOptions options, HeatingControlTrainingData data)
    {
        //var inaccuracyPercent = (float)_random.NextDouble() * options.SupplyTemperatureInaccuracy / 100f + 0.8f;
        var temperature = data.OutdoorTemperature;
        var insideOutsideTemperatureDifference = Math.Abs(data.PredictedOutdoorTemperature - data.OutdoorTemperature);
        if (data.PredictedOutdoorTemperature < temperature)
        {

            temperature -= insideOutsideTemperatureDifference * 0.5f;
        }

        //var insideOutsideTemperatureDifference = Math.Abs(data.PredictedOutdoorTemperature - data.OutdoorTemperature);
        var insideOutsideTemperature = temperature;


        var rtSoll = data.PreferredIndoorTemperature;
        var dar = insideOutsideTemperature - data.PreferredIndoorTemperature;
        if (insideOutsideTemperature >= data.PreferredIndoorTemperature)
            return 0;

        var vtSoll = rtSoll + options.Baseline - options.Gradient * dar * (1.4347f + 0.021f * dar + 247.9f * Math.Pow(10, -6) * Math.Pow(dar, 2));
        return Math.Clamp((float)vtSoll, insideOutsideTemperature, options.MaxSupplyTemperature);
    }
}
