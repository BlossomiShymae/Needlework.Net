using CommunityToolkit.Mvvm.ComponentModel;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class EndpointViewModel(string endpoint) : ObservableObject
    {
        public string Endpoint { get; } = endpoint;
        public string Title => $"Needlework.Net - {Endpoint}";
    }
}
