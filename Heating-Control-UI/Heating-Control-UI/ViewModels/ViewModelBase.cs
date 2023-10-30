using ReactiveUI;

namespace Heating_Control_UI.ViewModels;
public class ViewModelBase : ReactiveObject
{
    private bool _isPaneOpen = false;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
    }
}
