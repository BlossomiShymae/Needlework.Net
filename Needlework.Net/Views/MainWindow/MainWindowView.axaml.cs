using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Needlework.Net.ViewModels.MainWindow;
using Needlework.Net.ViewModels.Pages;
using Needlework.Net.ViewModels.Pages.About;
using Needlework.Net.ViewModels.Pages.Console;
using Needlework.Net.ViewModels.Pages.Endpoints;
using Needlework.Net.ViewModels.Pages.Home;
using Needlework.Net.ViewModels.Pages.Schemas;
using Needlework.Net.ViewModels.Pages.Settings;
using Needlework.Net.ViewModels.Pages.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Needlework.Net.Views.MainWindow;

public partial class MainWindowView : AppWindow
{
    public MainWindowView()
    {
        InitializeComponent();
    }

    public MainWindowView(MainWindowViewModel mainWindowViewModel, PageFactory pageFactory)
    {
        InitializeComponent();

        DataContext = mainWindowViewModel;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TransparencyLevelHint = [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None];
        Background = IsWindows11 ? null : Background;

        NavigationView.MenuItems = [.. new List<PageBase>()
        {
            pageFactory.GetPage<HomeViewModel>(),
            pageFactory.GetPage<EndpointsViewModel>(),
            pageFactory.GetPage<ConsoleViewModel>(),
            pageFactory.GetPage<WebSocketViewModel>(),
            pageFactory.GetPage<SchemasViewModel>(),
            pageFactory.GetPage<SettingsViewModel>(),
            pageFactory.GetPage<AboutViewModel>(),
        }
            .Select(ToNavigationViewItem)];
        NavigationView.GetObservable(NavigationView.SelectedItemProperty)
            .Subscribe(value =>
            {
                if (value is NavigationViewItem item)
                {
                    CurrentPageContentControl.Content = item.Tag;
                }
            });
        NavigationView.SelectedItem = NavigationView.MenuItems.Cast<NavigationViewItem>()
            .First();

        SchemaAutoCompleteBox.MinimumPopulateDelay = TimeSpan.FromSeconds(1);
        SchemaAutoCompleteBox.MinimumPrefixLength = 3;

        App.Current!.TryGetResource("TextFillColorPrimaryBrush", ActualThemeVariant, out var brush);
        CloseCommandBarButton.IconSource = new ImageIconSource
        {
            Source = new Projektanker.Icons.Avalonia.IconImage()
            {
                Value = "fa-solid fa-file-circle-xmark",
                Brush = (SolidColorBrush)brush!
            }
        };
    }

    private NavigationViewItem ToNavigationViewItem(PageBase page)
    {
        App.Current!.TryGetResource("TextFillColorPrimaryBrush", ActualThemeVariant, out var brush);
        return new NavigationViewItem()
        {
            Content = page.DisplayName,
            Tag = page,
            IconSource = new ImageIconSource
            {
                Source = new Projektanker.Icons.Avalonia.IconImage()
                {
                    Value = page.Icon,
                    Brush = (SolidColorBrush)brush!
                }
            }
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }
}