using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using Needlework.Net.Services;
using System;

namespace Needlework.Net.ViewModels.Pages.Endpoints
{
    public partial class EndpointSearchDetailsViewModel : ObservableObject
    {
        private readonly Document _document;

        private readonly Tab _tab;

        private readonly Action<ObservableObject> _onClicked;

        private readonly NotificationService _notificationService;

        public EndpointSearchDetailsViewModel(Services.NotificationService notificationService, Document document, Tab tab, Action<ObservableObject> onClicked, string? plugin)
        {
            _document = document;
            _tab = tab;
            _onClicked = onClicked;
            _plugin = plugin;
            _notificationService = notificationService;
        }

        [ObservableProperty]
        private string? _plugin;

        [RelayCommand]
        private void OpenEndpoint()
        {
            if (string.IsNullOrEmpty(Plugin)) return;
            _onClicked.Invoke(new PluginViewModel(_notificationService, Plugin, _document, _tab));
        }
    }
}
