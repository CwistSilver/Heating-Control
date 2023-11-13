using Avalonia.Controls;
using Heating_Control_UI.Utilities.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Linq;
using System.Reflection;

namespace Heating_Control_UI;
/// <summary>
/// Provides methods for configuring services and registering UI components in the Avalonia application.
/// </summary>
internal static class Entry
{
    /// <summary>
    /// Configures and adds essential services to the provided service collection.
    /// </summary>
    /// <param name="services">The service collection to which services and UI components will be added.</param>
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
        var reactiveObjects = assembly.GetTypes().Where(t => t.IsClass && typeof(ReactiveObject).IsAssignableFrom(t));
        foreach (var reactiveObject in reactiveObjects)
        {
            services.AddTransient(reactiveObject);
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
        var userControls = assembly.GetTypes().Where(t => t.IsClass && typeof(UserControl).IsAssignableFrom(t));
        foreach (var userControl in userControls)
        {
            services.AddTransient(userControl);
        }
    }
}
