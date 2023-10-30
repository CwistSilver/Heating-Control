using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Heating_Control;
using Heating_Control_UI.Utilities;
using Heating_Control_UI.ViewModels;
using Heating_Control_UI.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;

namespace Heating_Control_UI;
public partial class App : Application
{
    public ServiceProvider Services { get; private set; }
    ServiceCollection services = new ServiceCollection();
    public static PageNavigator Navigator => ((App)Current)._pageNavigator;
    private PageNavigator _pageNavigator;

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

        HeatingControlEntry.ConfigureServices(services);
        Entry.ConfigureServices(services);

        Services = services.BuildServiceProvider();

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };

            _pageNavigator = new PageNavigator(desktop.MainWindow, Services);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainView = new MainView();

            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

            _pageNavigator = new PageNavigator(mainView.PART_ContentPresenter, Services);

            singleViewPlatform.MainView = mainView;
        }

        if (!_isPrewview)
            Navigator.Push<HeatingControlView>();

        base.OnFrameworkInitializationCompleted();
    }
}