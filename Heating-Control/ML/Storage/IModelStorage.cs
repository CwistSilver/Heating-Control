using Heating_Control.Data;
using Microsoft.ML;

namespace Heating_Control.ML.Storage;
public interface IModelStorage
{
    ModelData? Load();
    void Save(ModelData modelData, DataViewSchema inputSchema);
}
