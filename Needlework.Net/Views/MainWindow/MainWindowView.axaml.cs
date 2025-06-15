using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using System;

namespace Needlework.Net.Views.MainWindow;

public partial class MainWindowView : AppWindow
{
    public MainWindowView()
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        TransparencyLevelHint = [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None];
        Background = IsWindows11 ? null : Background;

        SchemaAutoCompleteBox.MinimumPopulateDelay = TimeSpan.FromSeconds(1);
        SchemaAutoCompleteBox.MinimumPrefixLength = 3;

        CloseCommandBarButton.IconSource = new ImageIconSource
        {
            Source = new Projektanker.Icons.Avalonia.IconImage()
            {
                Value = "fa-solid fa-file-circle-xmark",
                Brush = new SolidColorBrush(Application.Current!.ActualThemeVariant.Key switch
                {
                    "Light" => Colors.Black,
                    "Dark" => Colors.White,
                    _ => Colors.Gray
                })
            }
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }
}