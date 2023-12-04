using Heating_Control.Data.Training;
using Tensorflow.Keras.Engine;

namespace Heating_Control.Training.Storage;
public interface ITrainingStorage
{
    string GetDataDirectorPath();
    string GetErrorFilePath(NeuralNetworkConfig config);
    string GetModelConfigDataFilePath(NeuralNetworkConfig config);
    string GetModelFilePath(NeuralNetworkConfig config);
    string GetStateFilePath(NeuralNetworkConfig config);
    IModel Load(NeuralNetworkConfig config);
    TrainingState? LoadState(NeuralNetworkConfig config);
    void SaveState(NeuralNetworkConfig config, TrainingState trainingState);
}