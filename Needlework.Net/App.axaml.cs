using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.Views.MainWindow;
using System;
using System.Text.Json;

namespace Needlework.Net;

public partial class App(IServiceProvider serviceProvider) : Application
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindowView()
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
            MainWindow = desktop.MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}