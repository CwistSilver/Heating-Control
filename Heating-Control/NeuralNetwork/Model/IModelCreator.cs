using Heating_Control.Data;

namespace Heating_Control.NeuralNetwork.Model;
public interface IModelCreator
{
    NeuralNetworkModel CreateModel();
}
