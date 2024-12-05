using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsViewModel : ObservableObject
{
    public IAvaloniaList<string> Plugins { get; }
    public IAvaloniaList<string> Query { get; }

    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private string? _selectedQuery = string.Empty;

    public Action<ObservableObject> OnClicked { get; }

    public EndpointsViewModel(IAvaloniaList<string> plugins, Action<ObservableObject> onClicked)
    {
        Plugins = plugins;
        Query = plugins;
        OnClicked = onClicked;
    }

    partial void OnSearchChanged(string value)
    {
        Query.Clear();
        if (!string.IsNullOrEmpty(Search))
            Query.AddRange(Plugins.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase)));
        else
            Query.AddRange(Plugins);
    }

    [RelayCommand]
    private void OpenEndpoint(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;

        OnClicked.Invoke(new EndpointViewModel(value));
    }
}
