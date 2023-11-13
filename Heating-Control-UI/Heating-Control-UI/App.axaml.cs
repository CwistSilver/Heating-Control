using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Heating_Control;
using Heating_Control_UI.Utilities.Navigation;
using Heating_Control_UI.Utilities.Storage;
using Heating_Control_UI.Views;
using Heating_Control_UI.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;

namespace Heating_Control_UI;
/// <summary>
/// The main application class for the Avalonia UI, responsible for initializing and managing the application's lifecycle.
/// </summary>
public partial class App : Application
{
    private const string PreviewStartupFunctionName = "SetupWithoutStarting";

    /// <summary>
    /// The service provider for dependency injection in the application.
    /// </summary>
    public ServiceProvider? Services { get; private set; }

    /// <summary>
    /// Provides global access to the page navigation service for the application.
    /// </summary>
    public static IPageNavigator Navigator => ((App)Current!)._pageNavigator!;
    private IPageNavigator? _pageNavigator;

    /// <summary>
    /// Provides global access to the application's storage service.
    /// </summary>
    public static IAppStorage Storage => ((App)Current!)._storage!;
    private IAppStorage? _storage;

    bool _isPrewview = false;

    /// <summary>
    /// Retrieves the top-level control for the current page in the navigator.
    /// </summary>
    /// <returns>The top-level control if available.</returns>
    public static TopLevel? GetTopLevel() => TopLevel.GetTopLevel(Navigator.CurrentPage);

    public override void Initialize()
    {
        try
        {
            var stackFrames = new StackTrace().GetFrames();
            _isPrewview = stackFrames.Any(i => i.GetMethod()!.Name.Contains(PreviewStartupFunctionName));
        }
        finally
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        var services = new ServiceCollection();
        HeatingControlEntry.ConfigureServices(services);
        Entry.ConfigureServices(services);

        Services = services.BuildServiceProvider();
        _storage = Services.GetRequiredService<IAppStorage>();
    }

    public override void OnFrameworkInitializationCompleted()
    {

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            _pageNavigator = new PageNavigator(((MainWindow)desktop.MainWindow).CarouselControl, Services!);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView();
            _pageNavigator = new PageNavigator(((MainView)singleViewPlatform.MainView).CarouselControl, Services!);
        }

        if (!_isPrewview)
            Navigator.Push<LoadingView>();

        base.OnFrameworkInitializationCompleted();
    }
}