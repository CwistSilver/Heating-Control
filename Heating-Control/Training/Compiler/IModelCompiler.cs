using Tensorflow.Keras.Engine;

namespace Heating_Control.Training.Compiler;
public interface IModelCompiler
{
    void CompileModel(IModel model);
}