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

public partial class EndpointView : UserControl, IRecipient<EditorUpdateMessage>, IRecipient<ContentRequestMessage>
{
    private TextEditor? _requestEditor;
    private TextEditor? _responseEditor;

    public EndpointView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var vm = (EndpointViewModel)DataContext!;
        _requestEditor = this.FindControl<TextEditor>("EndpointRequestEditor");
        _responseEditor = this.FindControl<TextEditor>("EndpointResponseEditor");
        _requestEditor?.ApplyJsonEditorSettings();
        _responseEditor?.ApplyJsonEditorSettings();

        WeakReferenceMessenger.Default.Register<EditorUpdateMessage>(this);
        WeakReferenceMessenger.Default.Register<ContentRequestMessage, string>(this, "EndpointRequestEditor");

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