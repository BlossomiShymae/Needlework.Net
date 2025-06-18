using Akavache;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Needlework.Net.Views.MainWindow;
using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net;

public partial class App : Application, IEnableLogger
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        this.Log()
            .Debug("NeedleworkDotNet version: {Version}", Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0");
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
        DataTemplates.Add(_serviceProvider.GetRequiredService<IDataTemplate>());
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        foreach (var page in _serviceProvider.GetServices<PageBase>())
        {
            Task.Run(page.InitializeAsync);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindowView()
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
            MainWindow = desktop.MainWindow;

            desktop.ShutdownRequested += (_, _) =>
            {
                var blobCache = _serviceProvider.GetRequiredService<IBlobCache>();
                blobCache.Flush().Wait();
                blobCache.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}