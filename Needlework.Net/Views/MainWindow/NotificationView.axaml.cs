using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.MainWindow;
using ReactiveUI;
using System.Reactive.Disposables;

namespace Needlework.Net.Views.MainWindow;

public partial class NotificationView : ReactiveUserControl<NotificationViewModel>
{
    public NotificationView()
    {
        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.Notification.Title, v => v.InfoBar.Title)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Notification.Message, v => v.InfoBar.Message)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.Notification.InfoBarSeverity, v => v.InfoBar.Severity)
                .DisposeWith(disposables);
            this.OneWayBind(ViewModel, vm => vm.IsButtonVisible, v => v.InfoBarButton.IsVisible)
                .DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.OpenUrlCommand, v => v.InfoBarButton);
        });

        InitializeComponent();
    }
}