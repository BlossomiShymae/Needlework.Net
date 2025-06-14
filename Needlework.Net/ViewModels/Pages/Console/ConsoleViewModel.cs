using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Console;

public partial class ConsoleViewModel : PageBase
{
    private readonly DocumentService _documentService;

    public ConsoleViewModel(DocumentService documentService, NotificationService notificationService) : base("Console", "fa-solid fa-terminal", -200)
    {
        _request = new(notificationService, Endpoints.Tab.LCU);
        _documentService = documentService;
    }

    public List<string> RequestMethods { get; } = ["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"];

    public List<string> RequestPaths { get; } = [];

    [ObservableProperty] private bool _isBusy = true;

    [ObservableProperty] private RequestViewModel _request;

    public override async Task InitializeAsync()
    {
        var document = await _documentService.GetLcuSchemaDocumentAsync();
        Dispatcher.UIThread.Invoke(() =>
        {
            RequestPaths.Clear();
            RequestPaths.AddRange(document.Paths);
        });
        IsBusy = false;
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        await Request.ExecuteAsync();
    }
}
