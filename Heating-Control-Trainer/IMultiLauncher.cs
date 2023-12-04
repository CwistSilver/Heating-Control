using Heating_Control.Data.Training;

namespace Heating_Control_Trainer;
public interface IMultiLauncher
{
    void Launch(List<NeuralNetworkConfig> configs);
}