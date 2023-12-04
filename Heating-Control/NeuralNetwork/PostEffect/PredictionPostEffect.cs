using Heating_Control.Data;

namespace Heating_Control.NeuralNetwork.PostEffect;
public sealed class PredictionPostEffect : IPredictionPostEffect
{
    public HeatingControlPrediction AddPostEffect(HeatingControlInputData input, float prediction)
    {
        var clampedPrediction = Math.Clamp(prediction, 0f, 1f) * Constants.MaxSupplyTemperature;

        var temperature = input.OutdoorTemperature;
        var insideOutsideTemperatureDifference = Math.Abs(input.PredictedOutdoorTemperature - input.OutdoorTemperature);
        if (input.PredictedOutdoorTemperature < temperature)
            temperature -= insideOutsideTemperatureDifference * 0.5f;

        if (temperature >= input.PreferredIndoorTemperature)
            return new HeatingControlPrediction() { SupplyTemperature = 0 };

        return new HeatingControlPrediction() { SupplyTemperature = clampedPrediction };
    }

}
