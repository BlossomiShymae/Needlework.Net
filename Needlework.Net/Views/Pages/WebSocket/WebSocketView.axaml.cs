using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Needlework.Net.Extensions;
using Needlework.Net.ViewModels.Pages.WebSocket;
using System;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.WebSocket;

public partial class WebSocketView : UserControl
{
    public WebSocketViewModel? _viewModel;
    private ListBox? _viewer;

    public WebSocketView()
    {
        InitializeComponent();

        ResponseEditor.ApplyJsonEditorSettings();
        OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _viewModel = (WebSocketViewModel)DataContext!;
        _viewer = this.FindControl<ListBox>("EventViewer");
        _viewModel.EventLog.CollectionChanged += EventLog_CollectionChanged;
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