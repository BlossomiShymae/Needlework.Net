using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointTabListViewModel : ReactiveObject
{
    public EndpointTabListViewModel(ObservableCollection<string> plugins, Action<ReactiveObject> onClicked, Models.Document document, Tab tab)
    {
        Plugins = new ObservableCollection<EndpointSearchDetailsViewModel>(plugins.Select(plugin => new EndpointSearchDetailsViewModel(document, tab, onClicked, plugin)));

        this.WhenAnyValue(x => x.Search)
            .Subscribe(search =>
            {
                EndpointSearchDetails.Clear();
                if (string.IsNullOrEmpty(search))
                {
                    EndpointSearchDetails.AddRange(
                        plugins.Where(plugin => plugin.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                        .Select(plugin => new EndpointSearchDetailsViewModel(document, tab, onClicked, plugin)));
                }
                else
                {
                    EndpointSearchDetails.AddRange(
                        plugins.Select(plugin => new EndpointSearchDetailsViewModel(document, tab, onClicked, plugin)));
                }
            });
    }

    public ObservableCollection<EndpointSearchDetailsViewModel> Plugins { get; }

    [Reactive]
    private ObservableCollection<EndpointSearchDetailsViewModel> _endpointSearchDetails = [];

    [Reactive]
    private string _search = string.Empty;
}
