using FluentAvalonia.UI.Windowing;

namespace Needlework.Net.Views.MainWindow;

public partial class MainWindowView : AppWindow
{
    public MainWindowView()
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
    }
}