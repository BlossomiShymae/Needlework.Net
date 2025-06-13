using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Needlework.Net.ViewModels.Pages.WebSocket;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using TextMateSharp.Grammars;

namespace Needlework.Net.Views.Pages.WebSocket;

public partial class WebSocketPage : ReactiveUserControl<WebSocketViewModel>
{
    public WebSocketPage()
    {
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.EventTypes, v => v.EventTypesComboBox.ItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.EventType, v => v.EventTypesComboBox.SelectedItem)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.Search, v => v.SearchTextBox.Text)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.IsAttach, v => v.IsAttachTextBox.IsChecked)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.IsTail, v => v.IsTailCheckBox.IsChecked)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.FilteredEventLog, v => v.EventListBox.ItemsSource)
                .DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedEventLog, v => v.EventListBox.SelectedItem)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.ClearCommand, v => v.ClearButton)
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.ViewModel!.GetEventTypesCommand)
                .SelectMany(x => x.Execute())
                .Subscribe()
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }

    //public void Receive(ResponseUpdatedMessage message)
    //{
    //    _responseEditor!.Text = message.Value;
    //}

    //protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    //{
    //    base.OnApplyTemplate(e);

    //    _viewModel = (WebSocketViewModel)DataContext!;
    //    _viewer = this.FindControl<ListBox>("EventViewer");
    //    _viewModel.EventLog.CollectionChanged += EventLog_CollectionChanged; ;

    //    _responseEditor = this.FindControl<TextEditor>("ResponseEditor");
    //    _responseEditor?.ApplyJsonEditorSettings();

    //    WeakReferenceMessenger.Default.Register(this, nameof(WebSocketViewModel));

    //    OnBaseThemeChanged(Application.Current!.ActualThemeVariant);
    //}

    //private void EventLog_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
    //    {
    //        if (_viewModel!.IsTail)
    //        {
    //            await _viewModel.EventLogLock.WaitAsync();
    //            try
    //            {
    //                _viewer!.ScrollIntoView(_viewModel.EventLog.Count - 1);
    //            }
    //            catch (InvalidOperationException) { }
    //            finally
    //            {
    //                _viewModel.EventLogLock.Release();
    //            }
    //        }
    //    });
    //}

    private void OnBaseThemeChanged(ThemeVariant currentTheme)
    {

        var registryOptions = new RegistryOptions(
            currentTheme == ThemeVariant.Dark ? ThemeName.DarkPlus : ThemeName.LightPlus);
    }
}