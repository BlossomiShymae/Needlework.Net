using Akavache;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Constants;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Needlework.Net.Views.MainWindow;
using System;
using System.Reactive.Linq;
using System.Text.Json;

namespace Needlework.Net;

public partial class App : Application, IEnableLogger
{
    private readonly IDataTemplate _viewLocator;

    private readonly IBlobCache _blobCache;

    private readonly PageFactory _pageFactory;

    private readonly MainWindowViewModel _mainWindowViewModel;

    public App(IServiceProvider serviceProvider)
    {
        _viewLocator = serviceProvider.GetRequiredService<IDataTemplate>();
        _blobCache = serviceProvider.GetRequiredService<IBlobCache>();
        _pageFactory = serviceProvider.GetRequiredService<PageFactory>();
        _mainWindowViewModel = serviceProvider.GetRequiredService<MainWindowViewModel>();

        this.Log()
            .Debug("NeedleworkDotNet version: {Version}", AppInfo.Version);
        this.Log()
            .Debug("OS description: {Description}", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
    }

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static readonly int MaxCharacters = 10_000;

    public static Window? MainWindow;

    public override void Initialize()
    {
        DataTemplates.Add(_viewLocator);
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindowView(_mainWindowViewModel, _pageFactory);
            MainWindow = desktop.MainWindow;
            desktop.ShutdownRequested += (_, _) =>
            {
                _blobCache.Flush().Wait();
                _blobCache.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}