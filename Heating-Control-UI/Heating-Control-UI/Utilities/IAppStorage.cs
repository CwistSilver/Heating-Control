using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities;
public interface IAppStorage
{
    public void AddOrSet(object value, [CallerMemberName] string key = "", bool composeKey = false);
    public T? Get<T>([CallerMemberName] string key = "", T? defaultValue = default, bool composeKey = false);

    Task Save();
}
