using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Extensions;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Serilog;
using System;
using System.IO;
using System.Reflection;

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

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("Logs/NeedleworkDotNet.log", rollingInterval: RollingInterval.Day, shared: true)
            .CreateLogger();
        logger.Debug("NeedleworkDotNet version: {Version}", Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0");
        logger.Debug("OS description: {Description}", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
        builder.AddLogging(builder => builder.AddSerilog(logger));

        var services = builder.BuildServiceProvider();
        return services;
    }

    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        File.WriteAllText($"errorlog-{DateTime.Now:HHmmssfff}", e.ExceptionObject.ToString());
    }
}
