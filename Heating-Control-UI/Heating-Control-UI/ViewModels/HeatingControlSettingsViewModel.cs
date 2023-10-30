using ReactiveUI;
using System.Threading.Tasks;

namespace Heating_Control_UI.ViewModels;
public class HeatingControlSettingsViewModel : ViewModelBase
{

    private float _gradient = 1.5f;
    public float Gradient
    {
        get => _gradient;
        set => this.RaiseAndSetIfChanged(ref _gradient, value);
    }

    private float _baseline = 1.5f;
    public float Baseline
    {
        get => _baseline;
        set => this.RaiseAndSetIfChanged(ref _baseline, value);
    }

    public void SaveAction()
    {
        App.Navigator.Pop();
    }

    public void CancelAction()
    {
        App.Navigator.Pop();
    }
}
