using DynamicData;
using Needlework.Net.Models;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public enum Tab
{
    LCU,
    GameClient
}

public partial class EndpointsViewModel : PageBase, IEnableLogger
{
    public record Endpoint(Document Document, Tab Tab);

    private readonly DataSource _dataSource;

    public EndpointsViewModel(IScreen? screen = null, DataSource? dataSource = null) : base("Endpoints", "list-alt", -500)
    {
        _dataSource = dataSource ?? Locator.Current.GetService<DataSource>()!;

        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;

        GetEndpointCommand.Subscribe(endpoint =>
        {
            Plugins.Clear();
            Plugins.AddRange(endpoint.Document.Plugins.Keys);

            var vm = new EndpointTabItemContentViewModel(Plugins, OnEndpointNavigation, endpoint.Document, endpoint.Tab);
            EndpointTabItems.Add(new(vm, vm.Title, null, true));
            IsBusy = false;
        });
        GetEndpointCommand.ThrownExceptions.Subscribe(ex =>
        {
            this.Log()
                .Error(ex, "Failed to get endpoint.");
            IsBusy = false;
        });
    }

    public override string? UrlPathSegment => "endpoints";

    public override ReactiveUI.IScreen HostScreen { get; }

    [Reactive]
    public ObservableCollection<string> Plugins { get; } = [];

    [Reactive]
    public ObservableCollection<EndpointTabItemViewModel> EndpointTabItems { get; } = [];

    [Reactive]
    private bool _isBusy = true;

    [ReactiveCommand]
    private async Task<Endpoint> GetEndpointAsync(Tab tab)
    {
        return tab switch
        {
            Tab.LCU => new(await _dataSource.GetLcuSchemaDocumentAsync(), tab),
            Tab.GameClient => new(await _dataSource.GetLolClientDocumentAsync(), tab),
            _ => throw new NotImplementedException(),
        };
    }

    private void OnEndpointNavigation(string? title, Guid guid)
    {
        foreach (var endpoint in EndpointTabItems)
        {
            if (endpoint.Content.Guid.Equals(guid))
            {
                endpoint.Header = endpoint.Content.Title;
                break;
            }
        }
    }
}