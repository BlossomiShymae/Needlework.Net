using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Extensions;
using Needlework.Net.Models;
using System.Diagnostics;

namespace Needlework.Net.ViewModels.MainWindow
{
    public partial class NotificationViewModel : ObservableObject, IEnableLogger
    {
        public NotificationViewModel(Notification notification)
        {
            Notification = notification;
            IsButtonVisible = !string.IsNullOrEmpty(notification.Url);
        }

        public bool IsButtonVisible { get; }

        public Notification Notification { get; }

        [RelayCommand]
        public void OpenUrl()
        {
            try
            {
                var url = Notification?.Url ?? "https://github.com/BlossomiShymae/Needlework.Net/releases/latest";
                var process = new Process() { StartInfo = new(url) { UseShellExecute = true } };
                process.Start();
            }
            catch (System.Exception exception)
            {
                this.Log()
                    .Error("Failed to open URL for notification: {@exception}", exception);
            }
        }
    }
}
