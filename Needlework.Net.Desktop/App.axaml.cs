using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Needlework.Net.Desktop.ViewModels;
using Needlework.Net.Desktop.Views;
using System;
using System.Text.Json;

namespace Needlework.Net.Desktop;

public partial class App(IServiceProvider serviceProvider) : Application
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true
    };

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}