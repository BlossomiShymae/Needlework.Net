using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace Needlework.Net.ViewModels.MainWindow
{
    public partial class NotificationViewModel : ReactiveObject
    {
        private IObservable<bool> _canExecute;

        public NotificationViewModel(Needlework.Net.Models.Notification notification)
        {
            Notification = notification;

            _canExecute = this.WhenAnyValue(x => x.Notification.Url)
                .Select(url => !string.IsNullOrEmpty(url));

            _isButtonVisibleHelper = _canExecute.ToProperty(this, x => x.IsButtonVisible);
        }

        [ObservableAsProperty]
        private bool _isButtonVisible = false;

        public Needlework.Net.Models.Notification Notification { get; }

        [ReactiveCommand(CanExecute = nameof(_canExecute))]
        public void OpenUrl()
        {
            var process = new Process() { StartInfo = new() { UseShellExecute = true } };
            process.Start();
        }
    }
}
