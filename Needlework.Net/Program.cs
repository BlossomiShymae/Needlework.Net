using Avalonia;
using Avalonia.ReactiveUI;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System;

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

        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .With(new Win32PlatformOptions { CompositionMode = [Win32CompositionMode.WinUIComposition, Win32CompositionMode.DirectComposition] })
                .With(new MacOSPlatformOptions { ShowInDock = true, })
                .LogToTrace()
                .UseReactiveUI();
    }

    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Logger.LogFatal(e);
    }
}
