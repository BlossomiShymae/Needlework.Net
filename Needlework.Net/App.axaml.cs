using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flurl.Http.Configuration;
using Needlework.Net.Converters;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Needlework.Net.ViewModels.Pages.About;
using Needlework.Net.ViewModels.Pages.Console;
using Needlework.Net.ViewModels.Pages.Endpoints;
using Needlework.Net.ViewModels.Pages.Home;
using Needlework.Net.ViewModels.Pages.WebSocket;
using Needlework.Net.Views.MainWindow;
using Needlework.Net.Views.Pages.About;
using Needlework.Net.Views.Pages.Console;
using Needlework.Net.Views.Pages.Endpoints;
using Needlework.Net.Views.Pages.Home;
using Needlework.Net.Views.Pages.WebSocket;
using ReactiveUI;
using Splat;
using Splat.Serilog;
using System.Text.Json;

namespace Needlework.Net;

public partial class App : Application
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static readonly int MaxCharacters = 10_000;

    public static Window? MainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RegisterValueConverters();
        RegisterAppServices();
        RegisterViews();
        RegisterViewModels();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow() { DataContext = Locator.Current.GetService<IScreen>() };
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterValueConverters()
    {
        Locator.CurrentMutable.RegisterConstant<IBindingTypeConverter>(new NullableToVisibilityConverter());
        Locator.CurrentMutable.RegisterConstant<IBindingTypeConverter>(new EnumerableToVisibilityConverter());
    }

    private static void RegisterViewModels()
    {
        Locator.CurrentMutable.RegisterConstant<PageBase>(new HomeViewModel());
        Locator.CurrentMutable.RegisterConstant<PageBase>(new EndpointsViewModel());
        Locator.CurrentMutable.RegisterConstant<PageBase>(new ConsoleViewModel());
        Locator.CurrentMutable.RegisterConstant<PageBase>(new WebSocketViewModel());
        Locator.CurrentMutable.RegisterConstant<PageBase>(new AboutViewModel());
        Locator.CurrentMutable.RegisterConstant<IScreen>(new MainWindowViewModel());
    }

    private static void RegisterViews()
    {
        Locator.CurrentMutable.Register<IViewFor<LibraryViewModel>>(() => new LibraryView());
        Locator.CurrentMutable.Register<IViewFor<NotificationViewModel>>(() => new NotificationView());
        Locator.CurrentMutable.Register<IViewFor<EventViewModel>>(() => new EventView());
        Locator.CurrentMutable.Register<IViewFor<EndpointTabListViewModel>>(() => new EndpointTabListView());
        Locator.CurrentMutable.Register<IViewFor<EndpointTabItemContentViewModel>>(() => new EndpointTabItemContentView());
        Locator.CurrentMutable.Register<IViewFor<EndpointSearchDetailsViewModel>>(() => new EndpointSearchDetailsView());
        Locator.CurrentMutable.Register<IViewFor<PluginViewModel>>(() => new PluginView());
        Locator.CurrentMutable.Register<IViewFor<PropertyClassViewModel>>(() => new PropertyClassView());
        Locator.CurrentMutable.Register<IViewFor<PathOperationViewModel>>(() => new PathOperationView());

        Locator.CurrentMutable.RegisterConstant<IViewFor<HomeViewModel>>(new HomePage());
        Locator.CurrentMutable.RegisterConstant<IViewFor<EndpointsViewModel>>(new EndpointsPage());
        Locator.CurrentMutable.RegisterConstant<IViewFor<ConsoleViewModel>>(new ConsolePage());
        Locator.CurrentMutable.RegisterConstant<IViewFor<WebSocketViewModel>>(new WebSocketPage());
        Locator.CurrentMutable.RegisterConstant<IViewFor<AboutViewModel>>(new AboutPage());
    }

    private static void RegisterAppServices()
    {
        Locator.CurrentMutable.UseSerilogFullLogger(Logger.Setup());
        Locator.CurrentMutable.RegisterConstant<IFlurlClientCache>(new FlurlClientCache()
            .Add("GithubClient", "https://api.github.com")
            .Add("GithubUserContentClient", "https://raw.githubusercontent.com"));
        Locator.CurrentMutable.RegisterConstant<NotificationService>(new NotificationService());
        Locator.CurrentMutable.RegisterConstant<DataSource>(new DataSource());
    }
}