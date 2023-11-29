using Heating_Control.Data;

namespace Heating_Control.NeuralNetwork.Model;
public interface IModelCreator
{
    public NeuralNetworkModel CurrentModel
    {
        get;
    }
    //NeuralNetworkModel CreateModel();
}
