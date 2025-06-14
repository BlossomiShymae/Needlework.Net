using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Needlework.Net.Extensions;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.Endpoints;

public partial class PluginView : UserControl
{
    public PluginView()
    {
        InitializeComponent();

        EndpointRequestEditor.ApplyJsonEditorSettings();
        EndpointResponseEditor.ApplyJsonEditorSettings();
        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
          currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}