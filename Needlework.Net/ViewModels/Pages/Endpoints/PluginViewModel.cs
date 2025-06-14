using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PluginViewModel : ObservableObject
{
    public PluginViewModel(Services.NotificationService notificationService, string endpoint, Models.Document document, Tab tab)
    {
        Endpoint = endpoint;
        PathOperations = document.Plugins[endpoint].Select(x => new PathOperationViewModel(notificationService, x, document, tab)).ToList();
        FilteredPathOperations = new ObservableCollection<PathOperationViewModel>(PathOperations);
    }

    public string Endpoint { get; }

    public string Title => Endpoint;

    public List<PathOperationViewModel> PathOperations { get; }

    [ObservableProperty]
    private ObservableCollection<PathOperationViewModel> _filteredPathOperations;

    [ObservableProperty]
    private PathOperationViewModel? _selectedPathOperation;

    [ObservableProperty]
    private string? _search;

    partial void OnSearchChanged(string? value)
    {
        FilteredPathOperations.Clear();

        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredPathOperations.AddRange(PathOperations);
            return;
        }
        FilteredPathOperations.AddRange(PathOperations.Where(o => o.Path.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
    }
}
