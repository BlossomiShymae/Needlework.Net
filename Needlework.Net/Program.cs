using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Extensions;
using Needlework.Net.Services;
using Needlework.Net.ViewModels;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;
using System.IO;

namespace Needlework.Net;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

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
        builder.AddSingletonsFromAssemblies<PageBase>();

        builder.AddHttpClient();

        var services = builder.BuildServiceProvider();
        return services;
    }

    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        File.WriteAllText($"errorlog-{DateTime.Now:HHmmssfff}", e.ExceptionObject.ToString());
    }
}
