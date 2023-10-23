using Microsoft.ML;

namespace Heating_Control.ML.Storage;
public interface IModelStorage
{
    ITransformer? Load();
    void Save(ITransformer transformer, DataViewSchema inputSchema);
}
