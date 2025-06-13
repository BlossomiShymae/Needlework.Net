using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.ObjectModel;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointTabItemContentViewModel : ReactiveObject
{
    private readonly Action<string?, Guid> _onEndpointNavigation;

    private readonly Tab _tab;

    public EndpointTabItemContentViewModel(ObservableCollection<string> plugins, Action<string?, Guid> onEndpointNavigation, Models.Document document, Tab tab)
    {
        _activeViewModel = _endpointsViewModel = new EndpointTabListViewModel(plugins, OnClicked, document, tab);
        _onEndpointNavigation = onEndpointNavigation;
        _tab = tab;
        _title = GetTitle(tab);
    }

    public Guid Guid { get; } = Guid.NewGuid();

    [Reactive]
    private ReactiveObject _activeViewModel;

    [Reactive]
    private ReactiveObject _endpointsViewModel;

    [Reactive]
    private string _title;

    private string GetTitle(Tab tab)
    {
        return tab switch
        {
            Tab.LCU => "LCU",
            Tab.GameClient => "Game Client",
            _ => string.Empty,
        };
    }

    private void OnClicked(ReactiveObject viewModel)
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
