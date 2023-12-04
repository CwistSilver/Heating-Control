namespace Heating_Control.NeuralNetwork;
public static class Constants
{
    public const float MaxSupplyTemperature = 100f;
    public const float MinSupplyTemperature = 70f;
    public const float MinOutdoorTemperature = -60f;
    public const float MaxOutdoorTemperature = 60f;
    public const float MinPredictedOutdoorTemperature = -60f;
    public const float MaxPredictedOutdoorTemperature = 60f;
    public const float MinPreferredIndoorTemperature = -15f;
    public const float MaxPreferredIndoorTemperature = 35f;
    public const float MinBaseline = 0f;
    public const float MaxBaseline = 20f;
    public const float MinGradient = 0f;
    public const float MaxGradient = 10f;
}