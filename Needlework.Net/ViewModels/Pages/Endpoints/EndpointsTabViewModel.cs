using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Needlework.Net.Messages;
using Needlework.Net.ViewModels.Shared;
using System;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class EndpointsTabViewModel : PageBase, IRecipient<DataReadyMessage>
{
    public IAvaloniaList<string> Plugins { get; } = new AvaloniaList<string>();
    public IAvaloniaList<EndpointItem> Endpoints { get; } = new AvaloniaList<EndpointItem>();

    [ObservableProperty] private bool _isBusy = true;

    private readonly ILogger<LcuRequestViewModel> _lcuRequestViewModelLogger;

    public EndpointsTabViewModel(ILogger<LcuRequestViewModel> lcuRequestViewModelLogger) : base("Endpoints", "list-alt", -500)
    {
        _lcuRequestViewModelLogger = lcuRequestViewModelLogger;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(DataReadyMessage message)
    {
        IsBusy = false;
        Plugins.Clear();
        Plugins.AddRange(message.Value.Plugins.Keys);

        Dispatcher.UIThread.Post(AddEndpoint);
    }

    [RelayCommand]
    private void AddEndpoint()
    {
        Endpoints.Add(new()
        {
            Content = new EndpointsNavigationViewModel(Plugins, OnEndpointNavigation, _lcuRequestViewModelLogger),
            Selected = true
        });
    }

    private void OnEndpointNavigation(string? title, Guid guid)
    {
        foreach (var endpoint in Endpoints)
        {
            if (endpoint.Content.Guid.Equals(guid))
            {
                endpoint.Header = title ?? "Endpoints";
                break;
            }
        }
    }
}

public partial class EndpointItem : ObservableObject
{
    [ObservableProperty] private string _header = "Endpoints";
    public IconSource IconSource { get; set; } = new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 20.0, Foreground = Avalonia.Media.Brushes.White };
    public bool Selected { get; set; } = false;
    public required EndpointsNavigationViewModel Content { get; init; }
}