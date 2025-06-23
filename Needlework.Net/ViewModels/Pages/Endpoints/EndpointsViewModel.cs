using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using Needlework.Net.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public enum Tab
{
    LCU,
    GameClient
}

public partial class EndpointsViewModel : PageBase
{
    private readonly DocumentService _documentService;

    private readonly NotificationService _notificationService;

    public EndpointsViewModel(DocumentService documentService, NotificationService notificationService) : base("Endpoints", "fa-solid fa-rectangle-list")
    {
        _documentService = documentService;
        _notificationService = notificationService;
    }

    public ObservableCollection<string> Plugins { get; } = [];

    public ObservableCollection<EndpointTabItemViewModel> Endpoints { get; } = [];

    [ObservableProperty] private bool _isBusy = true;

    public override async Task InitializeAsync()
    {
        await AddEndpoint(Tab.LCU);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task AddEndpoint(Tab tab)
    {
        Document document = tab switch
        {
            Tab.LCU => await _documentService.GetLcuSchemaDocumentAsync(),
            Tab.GameClient => await _documentService.GetLolClientDocumentAsync(),
            _ => throw new NotImplementedException(),
        };

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Plugins.Clear();
            Plugins.AddRange(document.Plugins.Keys);
            var vm = new EndpointTabItemContentViewModel(_notificationService, Plugins, OnEndpointNavigation, AddEndpointCommand, document, tab);
            Endpoints.Add(new()
            {
                Content = vm,
                Header = vm.Title,
                Selected = true
            });
        });
    }

    private void OnEndpointNavigation(string? title, Guid guid)
    {
        foreach (var endpoint in Endpoints)
        {
            if (endpoint.Content.Guid.Equals(guid))
            {
                endpoint.Header = endpoint.Content.Title;
                break;
            }
        }
    }
}
