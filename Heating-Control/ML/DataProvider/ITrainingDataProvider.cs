using Heating_Control.Data;

namespace Heating_Control.ML.DataProvider;
public interface ITrainingDataProvider
{
    /// <summary>
    /// Creates asynchronous random records based on the passed options.
    /// </summary>
    /// <param name="options">The options used for generating training data.</param>
    /// <returns>Realistic randomly generated records</returns>
    Task<IReadOnlyList<HeatingControlTrainingData>> GenerateAsync(TrainingDataOptions options);
}
