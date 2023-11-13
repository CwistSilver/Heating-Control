using Avalonia.Controls;
using Heating_Control_UI.Utilities.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Linq;
using System.Reflection;

namespace Heating_Control_UI;
internal class Entry
{
    internal static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<IAppStorage, AppStorage>();

        var assembly = Assembly.GetAssembly(typeof(App));
        RegisterWindows(assembly, services);
        RegisterUserControls(assembly, services);
        RegisterReactiveObjects(assembly, services);
    }

    private static void RegisterReactiveObjects(Assembly assembly, ServiceCollection services)
    {
        var windows = assembly.GetTypes().Where(t => t.IsClass && typeof(ReactiveObject).IsAssignableFrom(t));
        foreach (var window in windows)
        {
            services.AddTransient(window);
        }
    }

    private static void RegisterWindows(Assembly assembly, ServiceCollection services)
    {
        var windows = assembly.GetTypes().Where(t => t.IsClass && typeof(Window).IsAssignableFrom(t));
        foreach (var window in windows)
        {
            services.AddTransient(window);
        }
    }

    private static void RegisterUserControls(Assembly assembly, ServiceCollection services)
    {
        var views = assembly.GetTypes().Where(t => t.IsClass && typeof(UserControl).IsAssignableFrom(t));
        foreach (var view in views)
        {
            services.AddTransient(view);
        }
    }
}
