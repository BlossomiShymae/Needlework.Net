using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.WebSocket;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.Pages.WebSocket;

public partial class EventView : ReactiveUserControl<EventViewModel>
{
    public EventView()
    {
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.Time, v => v.TimeTextBlock.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Type, v => v.TypeTextBlock.Text)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Uri, v => v.UriTextBlock.Text)
                .DisposeWith(disposables);
        });

        InitializeComponent();
    }
}