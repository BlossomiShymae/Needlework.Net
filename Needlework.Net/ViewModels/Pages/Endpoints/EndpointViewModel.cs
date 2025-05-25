using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Needlework.Net.ViewModels.Shared;
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

    public event EventHandler<string>? PathOperationSelected;

    public EndpointViewModel(string endpoint, ILogger<LcuRequestViewModel> lcuRequestViewModelLogger, Models.Document lcuSchemaDocument)
    {
        Endpoint = endpoint;
        PathOperations = new AvaloniaList<PathOperationViewModel>(lcuSchemaDocument.Plugins[endpoint].Select(x => new PathOperationViewModel(x, lcuRequestViewModelLogger, lcuSchemaDocument)));
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
        PathOperationSelected?.Invoke(this, value.Operation.RequestTemplate ?? string.Empty);
    }
}
