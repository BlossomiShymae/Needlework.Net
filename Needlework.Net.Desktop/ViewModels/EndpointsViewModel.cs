using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.Core;
using Needlework.Net.Desktop.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class EndpointsViewModel : PageBase
    {
        public HttpClient HttpClient { get; }

        public DialogService DialogService { get; }

        [ObservableProperty] private List<string> _plugins = [];
        [ObservableProperty] private bool _isBusy = true;
        [ObservableProperty] private string _search = string.Empty;
        [ObservableProperty] private List<string> _query = [];
        [ObservableProperty] private string? _selectedQuery = string.Empty;

        public EndpointsViewModel(HttpClient httpClient, DialogService dialogService) : base("Endpoints", Material.Icons.MaterialIconKind.Hub, -500)
        {
            HttpClient = httpClient;
            DialogService = dialogService;

            Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            var handler = new LcuSchemaHandler(await Resources.GetOpenApiDocumentAsync(HttpClient));
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                Plugins = [.. handler.Plugins.Keys];
                Query = [.. Plugins];
                IsBusy = false;
            });
        }

        partial void OnSearchChanged(string value)
        {
            if (!string.IsNullOrEmpty(Search))
                Query = Plugins.Where(x => x.Contains(value)).ToList();
            else
                Query = Plugins;
        }

        partial void OnSelectedQueryChanged(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            DialogService.ShowEndpoint(value);
        }
    }
}
