using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointTabItemContentViewModel : ObservableObject
{
    private readonly Action<string?, Guid> _onEndpointNavigation;

    private readonly Tab _tab;

    public EndpointTabItemContentViewModel(Services.NotificationService notificationService, ObservableCollection<string> plugins, Action<string?, Guid> onEndpointNavigation, IAsyncRelayCommand addEndpointCommand, Models.Document document, Tab tab)
    {
        _activeViewModel = _endpointsViewModel = new EndpointListViewModel(notificationService, new ObservableCollection<string>(plugins), OnClicked, document, tab);
        _onEndpointNavigation = onEndpointNavigation;
        _tab = tab;
        _title = GetTitle(tab);

        AddEndpointCommand = addEndpointCommand;
    }

    public Guid Guid { get; } = Guid.NewGuid();

    public IAsyncRelayCommand AddEndpointCommand { get; }

    [ObservableProperty] private ObservableObject _activeViewModel;

    [ObservableProperty] private ObservableObject _endpointsViewModel;

    [ObservableProperty] private string _title;

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
        if (viewModel is PluginViewModel endpoint)
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
