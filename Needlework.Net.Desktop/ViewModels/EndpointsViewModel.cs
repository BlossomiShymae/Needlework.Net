using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Messages;
using System;
using System.Linq;
using System.Net.Http;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class EndpointsViewModel : ObservableObject, IRecipient<DataReadyMessage>
    {
        public HttpClient HttpClient { get; }

        public string Title => "Endpoints";
        public Action<ObservableObject> OnClicked;

        [ObservableProperty] private IAvaloniaReadOnlyList<string> _plugins = new AvaloniaList<string>();
        [ObservableProperty] private bool _isBusy = true;
        [ObservableProperty] private string _search = string.Empty;
        [ObservableProperty] private IAvaloniaReadOnlyList<string> _query = new AvaloniaList<string>();
        [ObservableProperty] private string? _selectedQuery = string.Empty;

        public EndpointsViewModel(HttpClient httpClient, Action<ObservableObject> onClicked)
        {
            HttpClient = httpClient;
            OnClicked = onClicked;

            WeakReferenceMessenger.Default.Register(this);
        }

        public void Receive(DataReadyMessage message)
        {
            IsBusy = false;
            Plugins = new AvaloniaList<string>([.. message.Value.Plugins.Keys]);
            Query = new AvaloniaList<string>([.. Plugins]);
        }

        partial void OnSearchChanged(string value)
        {
            if (!string.IsNullOrEmpty(Search))
                Query = new AvaloniaList<string>(Plugins.Where(x => x.Contains(value)));
            else
                Query = Plugins;
        }

        [RelayCommand]
        private void OpenEndpoint(string? value)
        {
            if (string.IsNullOrEmpty(value)) return;

            OnClicked.Invoke(new EndpointViewModel(value));
        }
    }
}
