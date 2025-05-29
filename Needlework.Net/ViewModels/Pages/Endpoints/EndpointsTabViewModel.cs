using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public enum Tab
{
    LCU,
    GameClient
}

public partial class EndpointsTabViewModel : PageBase
{
    public IAvaloniaList<string> Plugins { get; } = new AvaloniaList<string>();
    public IAvaloniaList<EndpointItem> Endpoints { get; } = new AvaloniaList<EndpointItem>();

    [ObservableProperty] private bool _isBusy = true;

    private readonly ILogger<RequestViewModel> _requestViewModelLogger;
    private readonly DataSource _dataSource;
    private readonly HttpClient _httpClient;

    public EndpointsTabViewModel(ILogger<RequestViewModel> requestViewModelLogger, DataSource dataSource, HttpClient httpClient) : base("Endpoints", "list-alt", -500)
    {
        _requestViewModelLogger = requestViewModelLogger;
        _dataSource = dataSource;
        _httpClient = httpClient;
    }
    public override async Task InitializeAsync()
    {
        await Dispatcher.UIThread.Invoke(async () => await AddEndpoint(Tab.LCU));
        IsBusy = false;
        IsInitialized = true;
    }

    [RelayCommand]
    private async Task AddEndpoint(Tab tab)
    {
        Document document = tab switch
        {
            Tab.LCU => await _dataSource.GetLcuSchemaDocumentAsync(),
            Tab.GameClient => await _dataSource.GetLolClientDocumentAsync(),
            _ => throw new NotImplementedException(),
        };

        Plugins.Clear();
        Plugins.AddRange(document.Plugins.Keys);

        var vm = new EndpointsNavigationViewModel(Plugins, OnEndpointNavigation, _requestViewModelLogger, document, tab, _httpClient);
        Endpoints.Add(new()
        {
            Content = vm,
            Header = vm.Title,
            Selected = true
        });
    }

    private void OnEndpointNavigation(string? title, Guid guid)
    {
        foreach (var endpoint in Endpoints)
        {
            if (endpoint.Content.Guid.Equals(guid))
            {
                endpoint.Header = endpoint.Content.Title;
                break;
            }
        }
    }
}

public partial class EndpointItem : ObservableObject
{
    [ObservableProperty] private string _header = string.Empty;
    public IconSource IconSource { get; set; } = new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 20.0, Foreground = Avalonia.Media.Brushes.White };
    public bool Selected { get; set; } = false;
    public required EndpointsNavigationViewModel Content { get; init; }
}