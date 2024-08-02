using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Desktop.Services;
using Needlework.Net.Desktop.ViewModels;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;
using System.Linq;

namespace Needlework.Net.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure(() => new App(BuildServices()))
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }

    private static IServiceProvider BuildServices()
    {
        var builder = new ServiceCollection();

        builder.AddSingleton<MainWindowViewModel>();
        builder.AddSingleton<DialogService>();
        // Dynamically add ViewModels
        var types = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(s => s.GetTypes())
          .Where(p => !p.IsAbstract && typeof(PageBase).IsAssignableFrom(p));
        foreach (var type in types)
            builder.AddSingleton(typeof(PageBase), type);

        builder.AddHttpClient();

        var services = builder.BuildServiceProvider();
        return services;
    }
}
