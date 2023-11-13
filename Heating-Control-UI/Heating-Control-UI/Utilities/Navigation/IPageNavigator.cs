using Avalonia.Animation;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Heating_Control_UI.Utilities.Navigation;
/// <summary>
/// Defines methods for navigating between pages in an application. 
/// </summary>
public interface IPageNavigator
{
    /// <summary>
    /// Gets the currently displayed page.
    /// </summary>
    ContentControl? CurrentPage { get; }

    /// <summary>
    /// Gets the stack of pages currently managed by the navigator.
    /// </summary>
    IReadOnlyList<ContentControl> PageStack { get; }

    /// <summary>
    /// Destroys the specified page.
    /// </summary>
    /// <param name="contentControl">The page to be destroyed.</param>
    void DestroyPage(ContentControl contentControl);

    /// <summary>
    /// Destroys a page of the specified type.
    /// </summary>
    /// <typeparam name="viewT">The type of the page to be destroyed.</typeparam>
    void DestroyPage<viewT>() where viewT : ContentControl;

    /// <summary>
    /// Pops the top page from the stack.
    /// </summary>
    /// <param name="pageTransition">The transition to use during the pop operation.</param>
    void Pop(IPageTransition? pageTransition = null);

    /// <summary>
    /// Asynchronously pops the top page from the stack.
    /// </summary>
    /// <param name="pageTransition">The transition to use during the pop operation.</param>
    /// <returns>A task representing the asynchronous pop operation.</returns>
    Task PopAsync(IPageTransition? pageTransition = null);

    /// <summary>
    /// Pushes a new page onto the stack.
    /// </summary>
    /// <param name="contentControl">The page to push onto the stack.</param>
    /// <param name="pageTransition">The transition to use during the push operation.</param>
    void Push(ContentControl contentControl, IPageTransition? pageTransition = null);

    /// <summary>
    /// Pushes <typeparamref name="viewT"/> onto the stack.
    /// </summary>
    /// <typeparam name="viewT">The type of the page to push onto the stack.</typeparam>
    /// <param name="pageTransition">The transition to use during the push operation.</param>
    /// <returns>The newly created and pushed <typeparamref name="viewT"/>.</returns>
    viewT Push<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl;

    /// <summary>
    /// Asynchronously pushes a new page onto the stack.
    /// </summary>
    /// <param name="contentControl">The page to push onto the stack.</param>
    /// <param name="pageTransition">The transition to use during the push operation.</param>
    /// <returns>A task representing the asynchronous push operation.</returns>
    Task PushAsync(ContentControl contentControl, IPageTransition? pageTransition = null);

    /// <summary>
    /// Asynchronously pushes <typeparamref name="viewT"/> onto the stack.
    /// </summary>
    /// <typeparam name="viewT">The type of the page to push onto the stack.</typeparam>
    /// <param name="pageTransition">The transition to use during the push operation.</param>
    /// <returns>A task representing the asynchronous push operation that returns the newly created <typeparamref name="viewT"/>.</returns>
    Task<viewT> PushAsync<viewT>(IPageTransition? pageTransition = null) where viewT : ContentControl;
}