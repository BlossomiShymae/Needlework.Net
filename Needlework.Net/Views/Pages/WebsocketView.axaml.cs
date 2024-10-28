using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Extensions;
using Needlework.Net.Messages;
using Needlework.Net.ViewModels.Pages.Websocket;
using System;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages;

public partial class WebsocketView : UserControl, IRecipient<ResponseUpdatedMessage>
{
    private TextEditor? _responseEditor;
    public WebsocketViewModel? _viewModel;
    private ListBox? _viewer;

    public WebsocketView()
    {
        InitializeComponent();
    }

    public void Receive(ResponseUpdatedMessage message)
    {
        _responseEditor!.Text = message.Value;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _viewModel = (WebsocketViewModel)DataContext!;
        _viewer = this.FindControl<ListBox>("EventViewer");
        _viewModel.EventLog.CollectionChanged += EventLog_CollectionChanged; ;

        _responseEditor = this.FindControl<TextEditor>("ResponseEditor");
        _responseEditor?.ApplyJsonEditorSettings();

        WeakReferenceMessenger.Default.Register(this, nameof(WebsocketViewModel));

        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    private void EventLog_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            if (_viewModel!.IsTail)
            {
                await _viewModel.EventLogLock.WaitAsync();
                try
                {
                    _viewer!.ScrollIntoView(_viewModel.EventLog.Count - 1);
                }
                catch (InvalidOperationException) { }
                finally
                {
                    _viewModel.EventLogLock.Release();
                }
            }
        });
    }

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {

        var registryOptions = new RegistryOptions(
            currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}