using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsViewModel : ObservableObject
{
    public IAvaloniaList<string> Plugins { get; }
    public IAvaloniaList<string> Query { get; }

    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private string? _selectedQuery = string.Empty;

    public Action<ObservableObject> OnClicked { get; }

    private readonly ILogger<LcuRequestViewModel> _lcuRequestViewModelLogger;
    private readonly Document _lcuSchemaDocument;

    public EndpointsViewModel(IAvaloniaList<string> plugins, Action<ObservableObject> onClicked, ILogger<LcuRequestViewModel> lcuRequestViewModelLogger, Models.Document lcuSchemaDocument)
    {
        Plugins = new AvaloniaList<string>(plugins);
        Query = new AvaloniaList<string>(plugins);
        OnClicked = onClicked;
        _lcuRequestViewModelLogger = lcuRequestViewModelLogger;
        _lcuSchemaDocument = lcuSchemaDocument;
    }

    partial void OnSearchChanged(string value)
    {
        Query.Clear();
        if (!string.IsNullOrEmpty(Search))
            Query.AddRange(Plugins.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
        else
            Query.AddRange(Plugins);
    }

    [RelayCommand]
    private void OpenEndpoint(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;

        OnClicked.Invoke(new EndpointViewModel(value, _lcuRequestViewModelLogger, _lcuSchemaDocument));
    }
}
