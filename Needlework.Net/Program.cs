using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Extensions;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;
using System.Threading.Tasks;

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
        var services = BuildServices();
        Task.Run(async () => await InitializeDataSourceAsync(services));

        return AppBuilder.Configure(() => new App(services))
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .With(new Win32PlatformOptions
                {
                    CompositionMode = [Win32CompositionMode.WinUIComposition, Win32CompositionMode.DirectComposition]
                });
    }

    private static async Task InitializeDataSourceAsync(IServiceProvider services)
    {
        var dataSource = services.GetRequiredService<DataSource>();
        await dataSource.InitializeAsync();
    }

    private static IServiceProvider BuildServices()
    {
        var builder = new ServiceCollection();

        builder.AddSingleton<MainWindowViewModel>();
        builder.AddSingleton<DialogService>();
        builder.AddSingleton<DataSource>();
        builder.AddSingletonsFromAssemblies<PageBase>();
        builder.AddHttpClient();
        builder.AddLogging(Logger.Setup);

        var services = builder.BuildServiceProvider();
        return services;
    }

    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.LogFatal(e);
    }
}
