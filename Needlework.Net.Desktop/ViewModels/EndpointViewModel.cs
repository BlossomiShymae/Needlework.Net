using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Messages;
using System.Linq;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class EndpointViewModel : ObservableObject
    {
        public string Endpoint { get; }
        public string Title => Endpoint;


        [ObservableProperty] private IAvaloniaReadOnlyList<PathOperationViewModel> _pathOperations;
        [ObservableProperty] private PathOperationViewModel? _selectedPathOperation;

        [ObservableProperty] private string? _search;
        [ObservableProperty] private IAvaloniaReadOnlyList<PathOperationViewModel> _filteredPathOperations;

        public EndpointViewModel(string endpoint)
        {
            Endpoint = endpoint;

            var handler = WeakReferenceMessenger.Default.Send<DataRequestMessage>().Response;
            PathOperations = new AvaloniaList<PathOperationViewModel>(handler.Plugins[endpoint].Select(x => new PathOperationViewModel(x)));
            FilteredPathOperations = new AvaloniaList<PathOperationViewModel>(PathOperations);
        }

        partial void OnSearchChanged(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredPathOperations = new AvaloniaList<PathOperationViewModel>(PathOperations);
                return;
            }

            FilteredPathOperations = new AvaloniaList<PathOperationViewModel>(PathOperations.Where(o => o.Path.ToLower().Contains(value.ToLower())));
        }

        partial void OnSelectedPathOperationChanged(PathOperationViewModel? value)
        {
            if (value == null) return;
            WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(value.Operation.RequestTemplate ?? string.Empty, "EndpointRequestEditor")));
        }
    }
}
