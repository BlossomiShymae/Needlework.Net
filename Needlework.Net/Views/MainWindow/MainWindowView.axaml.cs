using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace Needlework.Net.Views.MainWindow;

public partial class MainWindowView : AppWindow
{
    public MainWindowView()
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
        TransparencyLevelHint = [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None];
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (IsWindows11OrNewer())
            {
                Background = null;
            }
        }
    }

    private static bool IsWindows11OrNewer()
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;
    }
}