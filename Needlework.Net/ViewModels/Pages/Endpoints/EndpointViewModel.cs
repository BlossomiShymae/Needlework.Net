using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using System;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointViewModel : ObservableObject
{
    public string Endpoint { get; }
    public string Title => Endpoint;


    public IAvaloniaReadOnlyList<PathOperationViewModel> PathOperations { get; }
    [ObservableProperty] private PathOperationViewModel? _selectedPathOperation;

    [ObservableProperty] private string? _search;
    public IAvaloniaList<PathOperationViewModel> FilteredPathOperations { get; }

    public EndpointViewModel(string endpoint)
    {
        Endpoint = endpoint;

        var handler = WeakReferenceMessenger.Default.Send<DataRequestMessage>().Response;
        PathOperations = new AvaloniaList<PathOperationViewModel>(handler.Plugins[endpoint].Select(x => new PathOperationViewModel(x)));
        FilteredPathOperations = new AvaloniaList<PathOperationViewModel>(PathOperations);
    }

    partial void OnSearchChanged(string? value)
    {
        FilteredPathOperations.Clear();

        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredPathOperations.AddRange(PathOperations);
            return;
        }
        FilteredPathOperations.AddRange(PathOperations.Where(o => o.Path.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
    }

    partial void OnSelectedPathOperationChanged(PathOperationViewModel? value)
    {
        if (value == null) return;
        WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(value.Operation.RequestTemplate ?? string.Empty, "EndpointRequestEditor")));
    }
}
