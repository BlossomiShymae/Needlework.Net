using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Needlework.Net.ViewModels.Shared;
using System;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsNavigationViewModel : ObservableObject
{
    public Guid Guid { get; } = Guid.NewGuid();

    [ObservableProperty] private ObservableObject _activeViewModel;
    [ObservableProperty] private ObservableObject _endpointsViewModel;
    [ObservableProperty] private string _title;

    private readonly Action<string?, Guid> _onEndpointNavigation;
    private readonly Tab _tab;

    public EndpointsNavigationViewModel(IAvaloniaList<string> plugins, Action<string?, Guid> onEndpointNavigation, ILogger<RequestViewModel> requestViewModelLogger, Models.Document document, Tab tab, System.Net.Http.HttpClient httpClient)
    {
        _activeViewModel = _endpointsViewModel = new EndpointsViewModel(plugins, OnClicked, requestViewModelLogger, document, tab, httpClient);
        _onEndpointNavigation = onEndpointNavigation;
        _tab = tab;
        _title = GetTitle(tab);
    }

    private string GetTitle(Tab tab)
    {
        return tab switch
        {
            Tab.LCU => "LCU",
            Tab.GameClient => "Game Client",
            _ => string.Empty,
        };
    }

    private void OnClicked(ObservableObject viewModel)
    {
        ActiveViewModel = viewModel;
        if (viewModel is EndpointViewModel endpoint)
        {
            Title = $"{GetTitle(_tab)} - {endpoint.Title}";
            _onEndpointNavigation.Invoke(endpoint.Title, Guid);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        ActiveViewModel = EndpointsViewModel;
        Title = GetTitle(_tab);
        _onEndpointNavigation.Invoke(null, Guid);
    }
}
