using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Extensions;
using Needlework.Net.Desktop.Messages;
using Needlework.Net.Desktop.ViewModels;
using SukiUI;
using TextMateSharp.Grammars;

namespace Needlework.Net.Desktop.Views;

public partial class EndpointView : UserControl, IRecipient<EditorUpdateMessage>, IRecipient<ContentRequestMessage>
{
    private TextEditor? _requestEditor;
    private TextEditor? _responseEditor;

    public EndpointView()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var vm = (EndpointViewModel)DataContext!;
        _requestEditor = this.FindControl<TextEditor>("EndpointRequestEditor");
        _responseEditor = this.FindControl<TextEditor>("EndpointResponseEditor");
        _requestEditor?.ApplyJsonEditorSettings();
        _responseEditor?.ApplyJsonEditorSettings();

        WeakReferenceMessenger.Default.Register<EditorUpdateMessage>(this);
        WeakReferenceMessenger.Default.Register<ContentRequestMessage, string>(this, "EndpointRequestEditor");

        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
        SukiTheme.GetInstance().OnBaseThemeChanged += OnBaseThemeChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        WeakReferenceMessenger.Default.UnregisterAll(this);
        SukiTheme.GetInstance().OnBaseThemeChanged -= OnBaseThemeChanged;
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
          currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);

        var requestTmi = _requestEditor.InstallTextMate(registryOptions);
        requestTmi.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
            .GetLanguageByExtension(".json").Id));
        var responseTmi = _requestEditor.InstallTextMate(registryOptions);
        responseTmi.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
            .GetLanguageByExtension(".json").Id));
    }

    public void Receive(EditorUpdateMessage message)
    {
        switch (message.Value.Key)
        {
            case "EndpointRequestEditor":
                _requestEditor!.Text = message.Value.Text;
                break;
            case "EndpointResponseEditor":
                _responseEditor!.Text = message.Value.Text;
                break;
            default:
                break;
        }
    }

    public void Receive(ContentRequestMessage message)
    {
        message.Reply(_requestEditor!.Text);
    }
}