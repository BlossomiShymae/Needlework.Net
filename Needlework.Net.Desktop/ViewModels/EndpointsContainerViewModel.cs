using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Controls;
using System.Net.Http;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class EndpointsContainerViewModel : PageBase
    {
        [ObservableProperty] private ISukiStackPageTitleProvider _activeViewModel;

        public EndpointsContainerViewModel(HttpClient httpClient) : base("Endpoints", Material.Icons.MaterialIconKind.Hub, -500)
        {
            _activeViewModel = new EndpointsViewModel(httpClient, OnClicked);
        }

        private void OnClicked(ISukiStackPageTitleProvider viewModel)
        {
            ActiveViewModel = viewModel;
        }
    }
}
