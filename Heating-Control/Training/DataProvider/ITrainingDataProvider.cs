using Heating_Control.Data.Training;

namespace Heating_Control.Training.DataProvider;
public interface ITrainingDataProvider
{
    IReadOnlyList<HeatingControlTrainingData> Generate(int recordsToGenerate, bool randomOptions = true);
}