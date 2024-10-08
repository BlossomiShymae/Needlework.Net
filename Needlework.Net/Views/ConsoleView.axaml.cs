using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Extensions;
using Needlework.Net.Messages;
using Needlework.Net.ViewModels;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views;

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
        _responseEditor!.Text = message.Value;
    }

    public void Receive(ContentRequestMessage message)
    {
        message.Reply(_requestEditor!.Text);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _responseEditor = this.FindControl<TextEditor>("ResponseEditor");
        _requestEditor = this.FindControl<TextEditor>("RequestEditor");
        _responseEditor?.ApplyJsonEditorSettings();
        _requestEditor?.ApplyJsonEditorSettings();

        WeakReferenceMessenger.Default.Register<ResponseUpdatedMessage, string>(this, nameof(ConsoleViewModel));
        WeakReferenceMessenger.Default.Register<ContentRequestMessage, string>(this, "ConsoleRequestEditor");

        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {
        var registryOptions = new RegistryOptions(
            currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}