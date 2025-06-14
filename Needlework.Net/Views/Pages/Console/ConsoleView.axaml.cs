using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Needlework.Net.Extensions;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.Console;

public partial class ConsoleView : UserControl
{
    public ConsoleView()
    {
        InitializeComponent();

        ResponseEditor.ApplyJsonEditorSettings();
        RequestEditor.ApplyJsonEditorSettings();
        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
            currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}