using Heating_Control.Data;

namespace Heating_Control.Normalizer;
public interface IDataNormalizer
{
    float[] NormalizeInput(HeatingControlInputData inputData);
    float NormalizeOutput(float supplyTemperature);
}