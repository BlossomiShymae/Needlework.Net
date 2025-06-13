using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Subjects;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PluginViewModel : ReactiveObject
{
    private readonly Subject<string> _pathOperationSelectedSubject = new();

    public PluginViewModel(string endpoint, Models.Document document, Tab tab)
    {
        Endpoint = endpoint;
        PathOperations = [.. document.Plugins[endpoint].Select(x => new PathOperationViewModel(x, document, tab))];
        FilteredPathOperations = new ObservableCollection<PathOperationViewModel>(PathOperations);

        this.WhenAnyValue(x => x.Search)
            .Subscribe(search =>

            {
                FilteredPathOperations.Clear();
                if (string.IsNullOrWhiteSpace(search))
                {
                    FilteredPathOperations.AddRange(PathOperations);
                    return;
                }
                FilteredPathOperations.AddRange(PathOperations.Where(o => o.Path.Contains(search, StringComparison.InvariantCultureIgnoreCase)));
            });

        this.WhenAnyValue(x => x.SelectedPathOperation)
            .Subscribe(pathOperation =>
            {
                if (pathOperation == null) return;
                _pathOperationSelectedSubject.OnNext(pathOperation.Operation.RequestTemplate ?? string.Empty);
            });
    }

    public IObservable<string> PathOperationSelected { get { return _pathOperationSelectedSubject; } }

    public string Endpoint { get; }

    public string Title => Endpoint;

    public ObservableCollection<PathOperationViewModel> PathOperations { get; } = [];

    [Reactive]
    private ObservableCollection<PathOperationViewModel> _filteredPathOperations = [];

    [Reactive]
    private PathOperationViewModel? _selectedPathOperation;

    [Reactive]
    private string? _search;
}
