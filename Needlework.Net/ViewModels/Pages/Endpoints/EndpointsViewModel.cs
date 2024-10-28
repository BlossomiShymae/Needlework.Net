using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using System;
using System.Linq;
using System.Net.Http;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsViewModel : ObservableObject, IRecipient<DataReadyMessage>
{
    public HttpClient HttpClient { get; }

    public string Title => "Endpoints";
    public Action<ObservableObject> OnClicked;
    public IAvaloniaList<string> Plugins { get; } = new AvaloniaList<string>();
    public IAvaloniaList<string> Query { get; } = new AvaloniaList<string>();

    [ObservableProperty] private bool _isBusy = true;
    [ObservableProperty] private string _search = string.Empty;
    [ObservableProperty] private string? _selectedQuery = string.Empty;

    public EndpointsViewModel(HttpClient httpClient, Action<ObservableObject> onClicked)
    {
        HttpClient = httpClient;
        OnClicked = onClicked;

        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(DataReadyMessage message)
    {
        IsBusy = false;
        Plugins.Clear();
        Plugins.AddRange(message.Value.Plugins.Keys);
        Query.Clear();
        Query.AddRange(Plugins);
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
