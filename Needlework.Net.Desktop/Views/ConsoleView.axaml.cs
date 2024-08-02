using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using Needlework.Net.Desktop.ViewModels;
using SukiUI;
using System.Text.Json;
using TextMateSharp.Grammars;

namespace Needlework.Net.Desktop.Views;

public partial class ConsoleView : UserControl
{
    private TextEditor? _responseEditor;

    public ConsoleView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _responseEditor = this.FindControl<TextEditor>("ResponseEditor");
        _responseEditor!.TextArea.IndentationStrategy = new CSharpIndentationStrategy(_responseEditor.Options);
        _responseEditor!.TextArea.RightClickMovesCaret = true;
        _responseEditor!.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");

        ((ConsoleViewModel)DataContext!)!.ResponseBodyUpdated += ConsoleView_ResponseBodyUpdated;

        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
        SukiTheme.GetInstance().OnBaseThemeChanged += OnBaseThemeChanged;
    }

    private void ConsoleView_ResponseBodyUpdated(object? sender, TextUpdatedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Text))
            _responseEditor!.Text = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(e.Text), App.JsonSerializerOptions);
        else _responseEditor!.Text = e.Text;
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
            currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);

        var textMateInstallation = _responseEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
            .GetLanguageByExtension(".json").Id));
    }
}