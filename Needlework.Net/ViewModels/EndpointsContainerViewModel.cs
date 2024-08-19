using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http;

namespace Needlework.Net.ViewModels
{
    public partial class EndpointsContainerViewModel : PageBase
    {
        [ObservableProperty] private ObservableObject _activeViewModel;
        [ObservableProperty] private ObservableObject _endpointsViewModel;
        [ObservableProperty] private string _title = string.Empty;

        public EndpointsContainerViewModel(HttpClient httpClient) : base("Endpoints", "list-alt", -500)
        {
            _activeViewModel = _endpointsViewModel = new EndpointsViewModel(httpClient, OnClicked);
        }

        private void OnClicked(ObservableObject viewModel)
        {
            ActiveViewModel = viewModel;
            if (viewModel is EndpointViewModel endpoint) Title = endpoint.Title;
        }

        [RelayCommand]
        private void GoBack()
        {
            ActiveViewModel = EndpointsViewModel;
            Title = string.Empty;
        }
    }
}
