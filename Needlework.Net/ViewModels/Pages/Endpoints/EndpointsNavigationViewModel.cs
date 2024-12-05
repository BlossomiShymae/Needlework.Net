using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsNavigationViewModel : ObservableObject
{
    public Guid Guid { get; } = Guid.NewGuid();

    [ObservableProperty] private ObservableObject _activeViewModel;
    [ObservableProperty] private ObservableObject _endpointsViewModel;
    [ObservableProperty] private string _title = string.Empty;

    private readonly Action<string?, Guid> _onEndpointNavigation;

    public EndpointsNavigationViewModel(IAvaloniaList<string> plugins, Action<string?, Guid> onEndpointNavigation)
    {
        _activeViewModel = _endpointsViewModel = new EndpointsViewModel(plugins, OnClicked);
        _onEndpointNavigation = onEndpointNavigation;
    }

    private void OnClicked(ObservableObject viewModel)
    {
        ActiveViewModel = viewModel;
        if (viewModel is EndpointViewModel endpoint)
        {
            Title = endpoint.Title;
            _onEndpointNavigation.Invoke(endpoint.Title, Guid);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        ActiveViewModel = EndpointsViewModel;
        Title = string.Empty;
        _onEndpointNavigation.Invoke(null, Guid);
    }
}
