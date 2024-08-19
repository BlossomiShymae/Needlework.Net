using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using System;

namespace Needlework.Net.ViewModels
{
    public partial class InfoBarViewModel : ObservableObject
    {
        [ObservableProperty] private string _title;
        [ObservableProperty] private bool _isOpen;
        [ObservableProperty] private string _message;
        [ObservableProperty] private InfoBarSeverity _severity;
        [ObservableProperty] private TimeSpan _duration;
        [ObservableProperty] private Control? _actionButton;

        public InfoBarViewModel(string title, bool isOpen, string message, InfoBarSeverity severity, TimeSpan duration, Control? actionButton = null)
        {
            _title = title;
            _isOpen = isOpen;
            _message = message;
            _severity = severity;
            _duration = duration;
            _actionButton = actionButton;
        }
    }
}
