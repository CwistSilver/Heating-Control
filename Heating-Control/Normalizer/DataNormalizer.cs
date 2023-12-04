using Heating_Control.Data;
using Heating_Control.NeuralNetwork;

namespace Heating_Control.Normalizer;
public sealed class DataNormalizer : IDataNormalizer
{

    public float[] NormalizeInput(HeatingControlInputData inputData)
    {
        var nInput = new float[6];
        nInput[0] = Normalize(inputData.OutdoorTemperature, Constants.MinOutdoorTemperature, Constants.MaxOutdoorTemperature, 0f, 1f);
        nInput[1] = Normalize(inputData.PredictedOutdoorTemperature, Constants.MinPredictedOutdoorTemperature, Constants.MaxPredictedOutdoorTemperature, 0f, 1f);
        nInput[2] = Normalize(inputData.PreferredIndoorTemperature, Constants.MinPreferredIndoorTemperature, Constants.MaxPreferredIndoorTemperature, 0f, 1f);

        nInput[3] = Normalize(inputData.Baseline, Constants.MinBaseline, Constants.MaxBaseline, 0f, 1f);
        nInput[4] = Normalize(inputData.Gradient, Constants.MinGradient, Constants.MaxGradient, 0f, 1f);
        nInput[5] = Normalize(inputData.MaxSupplyTemperature, Constants.MinSupplyTemperature, Constants.MaxSupplyTemperature, 0f, 1f);

        return nInput;
    }

    public float NormalizeOutput(float supplyTemperature)
    {
        return supplyTemperature / 100f;
    }

    private static float Normalize(float val, float valMin, float valMax, float min, float max) => (val - valMin) / (valMax - valMin) * (max - min) + min;
}
