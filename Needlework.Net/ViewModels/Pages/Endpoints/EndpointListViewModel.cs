using Avalonia;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using Needlework.Net.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointListViewModel : ObservableObject
{
    private readonly Document _document;

    private readonly Tab _tab;

    private readonly Action<ObservableObject> _onClicked;

    private readonly ObservableCollection<string> _plugins;

    private readonly NotificationService _notificationService;

    public EndpointListViewModel(NotificationService notificationService, ObservableCollection<string> plugins, Action<ObservableObject> onClicked, Models.Document document, Tab tab)
    {
        _plugins = new ObservableCollection<string>(plugins);
        _document = document;
        _tab = tab;
        _onClicked = onClicked;
        _notificationService = notificationService;

        Plugins = EndpointSearchDetails = new ObservableCollection<EndpointSearchDetailsViewModel>(plugins.Select(plugin => new EndpointSearchDetailsViewModel(notificationService, document, tab, onClicked, plugin)));
    }

    public ObservableCollection<EndpointSearchDetailsViewModel> Plugins { get; }

    [ObservableProperty]
    private ObservableCollection<EndpointSearchDetailsViewModel> _endpointSearchDetails = [];

    [ObservableProperty]
    private string _search = string.Empty;

    [ObservableProperty]
    private Vector _offset = new();

    partial void OnSearchChanged(string value)
    {
        EndpointSearchDetails.Clear();
        if (!string.IsNullOrEmpty(Search))
        {
            EndpointSearchDetails.AddRange(_plugins.Where(plugin => plugin.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(plugin => new EndpointSearchDetailsViewModel(_notificationService, _document, _tab, _onClicked, plugin)));
        }
        else
        {
            EndpointSearchDetails.AddRange(
                _plugins.Select(plugin => new EndpointSearchDetailsViewModel(_notificationService, _document, _tab, _onClicked, plugin)));
        }
    }

    [RelayCommand]
    private void OpenEndpoint(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        _onClicked.Invoke(new PluginViewModel(_notificationService, value, _document, _tab));
    }
}
