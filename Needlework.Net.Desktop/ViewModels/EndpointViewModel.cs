using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Messages;
using SukiUI.Controls;
using System.Linq;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class EndpointViewModel : ObservableObject, ISukiStackPageTitleProvider
    {
        public string Endpoint { get; }
        public string Title => Endpoint;

        [ObservableProperty] private IAvaloniaReadOnlyList<PathOperationViewModel> _pathOperations;
        [ObservableProperty] private PathOperationViewModel? _selectedPathOperation;

        public EndpointViewModel(string endpoint)
        {
            Endpoint = endpoint;

            var handler = WeakReferenceMessenger.Default.Send<DataRequestMessage>().Response;
            PathOperations = new AvaloniaList<PathOperationViewModel>(handler.Plugins[endpoint].Select(x => new PathOperationViewModel(x)));
        }
    }
}
