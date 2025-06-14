using FluentAvalonia.UI.Controls;
using Needlework.Net.Models;
using System;
using System.Reactive.Subjects;

namespace Needlework.Net.Services
{
    public class NotificationService
    {
        private readonly Subject<Notification> _notificationSubject = new();

        public IObservable<Notification> Notifications { get { return _notificationSubject; } }

        public void Notify(string title, string message, InfoBarSeverity severity, TimeSpan? duration = null, string? url = null)
        {
            var notification = new Notification(title, message, severity, duration, url);
            _notificationSubject.OnNext(notification);
        }
    }
}
