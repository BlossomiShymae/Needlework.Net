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
using System.Text.Json;
using TextMateSharp.Grammars;

namespace Needlework.Net.Desktop.Views;

public partial class ConsoleView : UserControl, IRecipient<ResponseUpdatedMessage>, IRecipient<ContentRequestMessage>
{
    private TextEditor? _responseEditor;
    private TextEditor? _requestEditor;

    public ConsoleView()
    {
        InitializeComponent();
    }

    public void Receive(ResponseUpdatedMessage message)
    {
        if (!string.IsNullOrEmpty(message.Value))
        {
            var text = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(message.Value), App.JsonSerializerOptions);
            if (text.Length >= App.MaxCharacters)
            {
                WeakReferenceMessenger.Default.Send(new OopsiesWindowRequestedMessage(text), nameof(ConsoleView));
                _responseEditor!.Text = string.Empty;
            }
            else _responseEditor!.Text = text;
        }
        else _responseEditor!.Text = message.Value;
    }

    public void Receive(ContentRequestMessage message)
    {
        message.Reply(_requestEditor!.Text);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _responseEditor = this.FindControl<TextEditor>("ResponseEditor");
        _requestEditor = this.FindControl<TextEditor>("RequestEditor");
        _responseEditor?.ApplyJsonEditorSettings();
        _requestEditor?.ApplyJsonEditorSettings();

        WeakReferenceMessenger.Default.Register<ResponseUpdatedMessage, string>(this, nameof(ConsoleViewModel));
        WeakReferenceMessenger.Default.Register<ContentRequestMessage, string>(this, "ConsoleRequestEditor");

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

        var responseTmi = _responseEditor.InstallTextMate(registryOptions);
        responseTmi.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
            .GetLanguageByExtension(".json").Id));
        var requestTmi = _requestEditor.InstallTextMate(registryOptions);
        requestTmi.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions
            .GetLanguageByExtension(".json").Id));
    }
}