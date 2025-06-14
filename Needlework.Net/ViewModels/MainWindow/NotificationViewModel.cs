using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using System.Diagnostics;

namespace Needlework.Net.ViewModels.MainWindow
{
    public partial class NotificationViewModel : ObservableObject
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
            var process = new Process() { StartInfo = new() { UseShellExecute = true } };
            process.Start();
        }
    }
}
