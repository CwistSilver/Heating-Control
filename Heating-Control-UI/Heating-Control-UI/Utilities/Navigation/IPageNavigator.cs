using Avalonia.Animation;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities.Navigation;
public interface IPageNavigator
{
    ContentControl? CurrentPage { get; }
    IReadOnlyList<ContentControl> PageStack { get; }

    void DestroyPage(ContentControl contentControl);
    void DestroyPage<viewT>() where viewT : ContentControl;
    void Pop(IPageTransition? pageTransition = null);
    Task PopAsync(IPageTransition? pageTransition = null);
    void Push(ContentControl contentControl, IPageTransition? pageTransition = null);
    viewT Push<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl;
    Task PushAsync(ContentControl contentControl, IPageTransition? pageTransition = null);
    Task<viewT> PushAsync<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl;
}