using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Heating_Control;
using Heating_Control_UI.Utilities;
using Heating_Control_UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;

namespace Heating_Control_UI;
public partial class App : Application
{
    public ServiceProvider Services { get; private set; }
    ServiceCollection services = new ServiceCollection();
    public static PageNavigator Navigator => ((App)Current!)._pageNavigator;
    private PageNavigator _pageNavigator;

    public static IAppStorage Storage => ((App)Current!)._storage;
    private IAppStorage _storage;

    public static T? GetResourceFromThemeDictionarie<T>(string name)
    {
        var mergedDictionaries = (ResourceDictionary)Current!.Resources.MergedDictionaries[0];

        if (!mergedDictionaries.ThemeDictionaries.TryGetValue(Current.ActualThemeVariant, out var themeVariantProvider))
            return default;
        if (!themeVariantProvider.TryGetResource(name, null, out var resource))
            return default;

        return (T?)resource;
    }


    public static TopLevel? GetTopLevel()
    {
        return TopLevel.GetTopLevel(Navigator.CurrentPage);
    }

    bool _isPrewview = false;
    public override void Initialize()
    {
        StackTrace stackTrace = new StackTrace();
        StackFrame[] stackFrames = stackTrace.GetFrames();
        _isPrewview = stackFrames.Any(i => i.GetMethod().Name.Contains("SetupWithoutStarting"));
             

        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        HeatingControlEntry.ConfigureServices(services);
        Entry.ConfigureServices(services);

        Services = services.BuildServiceProvider();
        _storage = Services.GetRequiredService<IAppStorage>();
    }

    public override void OnFrameworkInitializationCompleted()
    {

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;

            _pageNavigator = new PageNavigator(mainWindow.CarouselControl, Services);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainView = new MainView();

            singleViewPlatform.MainView = new MainView();
            _pageNavigator = new PageNavigator(mainView.CarouselControl, Services);

            singleViewPlatform.MainView = mainView;
        }

        if (!_isPrewview)
            Navigator.Push<LoadingView>();

        base.OnFrameworkInitializationCompleted();
    }
}