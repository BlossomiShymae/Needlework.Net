using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsTabViewModel : PageBase
{
    public IAvaloniaList<string> Plugins { get; } = new AvaloniaList<string>();
    public IAvaloniaList<EndpointItem> Endpoints { get; } = new AvaloniaList<EndpointItem>();

    [ObservableProperty] private bool _isBusy = true;

    private readonly ILogger<LcuRequestViewModel> _lcuRequestViewModelLogger;
    private readonly DataSource _dataSource;

    public EndpointsTabViewModel(ILogger<LcuRequestViewModel> lcuRequestViewModelLogger, DataSource dataSource) : base("Endpoints", "list-alt", -500)
    {
        _lcuRequestViewModelLogger = lcuRequestViewModelLogger;
        _dataSource = dataSource;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }
    public override async Task InitializeAsync()
    {
        var document = await _dataSource.GetLcuSchemaDocumentAsync();
        Plugins.Clear();
        Plugins.AddRange(document.Plugins.Keys);
        await Dispatcher.UIThread.Invoke(AddEndpoint);
        IsBusy = false;
        IsInitialized = true;
    }

    [RelayCommand]
    private async Task AddEndpoint()
    {
        var lcuSchemaDocument = await _dataSource.GetLcuSchemaDocumentAsync();
        Endpoints.Add(new()
        {
            Content = new EndpointsNavigationViewModel(Plugins, OnEndpointNavigation, _lcuRequestViewModelLogger, lcuSchemaDocument),
            Selected = true
        });
    }

    private void OnEndpointNavigation(string? title, Guid guid)
    {
        foreach (var endpoint in Endpoints)
        {
            if (endpoint.Content.Guid.Equals(guid))
            {
                endpoint.Header = title ?? "Endpoints";
                break;
            }
        }
    }
}

public partial class EndpointItem : ObservableObject
{
    [ObservableProperty] private string _header = "Endpoints";
    public IconSource IconSource { get; set; } = new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 20.0, Foreground = Avalonia.Media.Brushes.White };
    public bool Selected { get; set; } = false;
    public required EndpointsNavigationViewModel Content { get; init; }
}