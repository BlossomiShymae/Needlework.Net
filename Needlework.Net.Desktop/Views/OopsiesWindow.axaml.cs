using FluentAvalonia.UI.Windowing;

namespace Needlework.Net.Desktop.Views;

public partial class OopsiesWindow : AppWindow
{
    public OopsiesWindow()
    {
        InitializeComponent();

        TitleBar.ExtendsContentIntoTitleBar = true;
    }
}