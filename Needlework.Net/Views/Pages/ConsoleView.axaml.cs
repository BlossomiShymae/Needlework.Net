using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaEdit;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.Pages;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages;

public partial class ConsoleView : UserControl
{
    private TextEditor? _responseEditor;
    private TextEditor? _requestEditor;

    public ConsoleView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _responseEditor = this.FindControl<TextEditor>("ResponseEditor");
        _requestEditor = this.FindControl<TextEditor>("RequestEditor");
        _responseEditor?.ApplyJsonEditorSettings();
        _requestEditor?.ApplyJsonEditorSettings();

        var vm = (ConsoleViewModel)DataContext!;
        vm.Request.RequestText += LcuRequest_RequestText; ;
        vm.Request.UpdateText += LcuRequest_UpdateText;

        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    private void LcuRequest_RequestText(object? sender, ViewModels.Shared.RequestViewModel e)
    {
        e.RequestBody = _requestEditor!.Text;
    }

    private void LcuRequest_UpdateText(object? sender, string e)
    {
        _responseEditor!.Text = e;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        var vm = (ConsoleViewModel)DataContext!;
        vm.Request.RequestText -= LcuRequest_RequestText;
        vm.Request.UpdateText -= LcuRequest_UpdateText;
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
            currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}