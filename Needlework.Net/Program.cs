using Akavache;
using Akavache.Sqlite3;
using Avalonia;
using Avalonia.Controls.Templates;
using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Extensions;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Needlework.Net.ViewModels.Pages.About;
using Needlework.Net.ViewModels.Pages.Console;
using Needlework.Net.ViewModels.Pages.Endpoints;
using Needlework.Net.ViewModels.Pages.Home;
using Needlework.Net.ViewModels.Pages.Schemas;
using Needlework.Net.ViewModels.Pages.WebSocket;
using Needlework.Net.Views.MainWindow;
using Needlework.Net.Views.Pages.About;
using Needlework.Net.Views.Pages.Console;
using Needlework.Net.Views.Pages.Endpoints;
using Needlework.Net.Views.Pages.Home;
using Needlework.Net.Views.Pages.Schemas;
using Needlework.Net.Views.Pages.WebSocket;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Serilog;
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
        IconProvider.Current.Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure(() => new App(BuildServices()))
                .UsePlatformDetect()
                .WithInterFont()
                .With(new Win32PlatformOptions { CompositionMode = [Win32CompositionMode.WinUIComposition, Win32CompositionMode.DirectComposition] })
                .With(new MacOSPlatformOptions { ShowInDock = true, })
                .LogToTrace();
    }

    private static IServiceProvider BuildServices()
    {
        var builder = new ServiceCollection();

        AddViews(builder);
        AddViewModels(builder);
        AddServices(builder);

        return builder.BuildServiceProvider();
    }

    private static void AddViews(ServiceCollection builder)
    {
        var locator = new ViewLocator();
        // MAIN WINDOW
        locator.Register<NotificationViewModel>(() => new NotificationView());
        locator.Register<ViewModels.MainWindow.SchemaSearchDetailsViewModel>(() => new Views.MainWindow.SchemaSearchDetailsView());
        locator.Register<SchemaViewModel>(() => new SchemaView());
        // ABOUT
        locator.Register<AboutViewModel>(() => new AboutView());
        // CONSOLE
        locator.Register<ConsoleViewModel>(() => new ConsoleView());
        // ENDPOINTS
        locator.Register<EndpointListViewModel>(() => new EndpointListView());
        locator.Register<EndpointSearchDetailsViewModel>(() => new EndpointSearchDetailsView());
        locator.Register<EndpointsViewModel>(() => new EndpointsView());
        locator.Register<EndpointTabItemContentViewModel>(() => new EndpointTabItemContentView());
        locator.Register<PathOperationViewModel>(() => new PathOperationView());
        locator.Register<PluginViewModel>(() => new PluginView());
        locator.Register<PropertyClassViewModel>(() => new PropertyClassView());
        // HOME
        locator.Register<HomeViewModel>(() => new HomeView());
        locator.Register<LibraryViewModel>(() => new LibraryView());
        locator.Register<HextechDocsPostViewModel>(() => new HextechDocsPostView());
        // SCHEMAS
        locator.Register<SchemasViewModel>(() => new SchemasView());
        locator.Register<ViewModels.Pages.Schemas.SchemaSearchDetailsViewModel>(() => new Views.Pages.Schemas.SchemaSearchDetailsView());
        // WEBSOCKET
        locator.Register<WebSocketViewModel>(() => new WebSocketView());
        locator.Register<EventViewModel>(() => new EventView());

        builder.AddSingleton<IDataTemplate>(locator);
    }

    private static void AddServices(ServiceCollection builder)
    {
        builder.AddSingleton<DialogService>();
        builder.AddSingleton<DocumentService>();
        builder.AddSingleton<NotificationService>();
        builder.AddSingleton<SchemaPaneService>();
        builder.AddSingleton<HextechDocsPostService>();
        builder.AddSingleton<IBlobCache>((_) =>
        {
            Directory.CreateDirectory("Data");
            return new SqlRawPersistentBlobCache("Data/data.sqlite");
        });
        builder.AddSingleton<IFlurlClientCache>(new FlurlClientCache()
            .Add("GithubClient", "https://api.github.com")
            .Add("GithubUserContentClient", "https://raw.githubusercontent.com")
            .Add("Client"));

        builder.AddLogging((builder) => builder.AddSerilog(EnableLoggerExtensions.Log(null)));
    }

    private static void AddViewModels(ServiceCollection builder)
    {
        builder.AddSingleton<MainWindowViewModel>();

        builder.AddSingleton<PageBase, HomeViewModel>();
        builder.AddSingleton<PageBase, ConsoleViewModel>();
        builder.AddSingleton<PageBase, EndpointsViewModel>();
        builder.AddSingleton<PageBase, WebSocketViewModel>();
        builder.AddSingleton<PageBase, SchemasViewModel>();
        builder.AddSingleton<PageBase, AboutViewModel>();
    }

    private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        File.WriteAllText($"Logs/fatal-{DateTime.Now:yyyyMMdd}.log", e.ExceptionObject.ToString());
    }
}
