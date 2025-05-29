using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Linq;
using System.Net.Http;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsViewModel : ObservableObject
{
    public IAvaloniaList<string> Plugins { get; }
    public IAvaloniaList<string> Query { get; }

    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private string? _selectedQuery = string.Empty;

    public Action<ObservableObject> OnClicked { get; }

    private readonly ILogger<RequestViewModel> _requestViewModelLogger;
    private readonly Document _document;
    private readonly Tab _tab;
    private readonly HttpClient _httpClient;

    public EndpointsViewModel(IAvaloniaList<string> plugins, Action<ObservableObject> onClicked, ILogger<RequestViewModel> requestViewModelLogger, Models.Document document, Tab tab, System.Net.Http.HttpClient httpClient)
    {
        Plugins = new AvaloniaList<string>(plugins);
        Query = new AvaloniaList<string>(plugins);
        OnClicked = onClicked;
        _requestViewModelLogger = requestViewModelLogger;
        _document = document;
        _tab = tab;
        _httpClient = httpClient;
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

        OnClicked.Invoke(new EndpointViewModel(value, _requestViewModelLogger, _document, _tab, _httpClient));
    }
}
