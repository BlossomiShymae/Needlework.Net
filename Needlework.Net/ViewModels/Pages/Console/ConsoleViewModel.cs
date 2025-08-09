using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Extensions;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Console;

public partial class ConsoleViewModel : PageBase, IEnableLogger
{
    private readonly DocumentService _documentService;

    private readonly NotificationService _notificationService;

    public ConsoleViewModel(DocumentService documentService, NotificationService notificationService) : base("Console", "fa-solid fa-terminal")
    {
        _request = new(notificationService, Endpoints.Tab.LCU);
        _documentService = documentService;
        _notificationService = notificationService;
    }

    public List<string> RequestMethods { get; } = ["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"];

    public List<string> RequestPaths { get; } = [];

    [ObservableProperty] private bool _isBusy = true;

    [ObservableProperty] private RequestViewModel _request;

    public override async Task InitializeAsync()
    {
        try
        {
            var document = await _documentService.GetLcuSchemaDocumentAsync();
            Dispatcher.UIThread.Invoke(() =>
            {
                RequestPaths.Clear();
                RequestPaths.AddRange(document.Paths);
            });
            IsBusy = false;
        }
        catch (Exception ex)
        {
            this.Log()
                .Error(ex, "Failed to load console.");
            _notificationService.Notify("Console", ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error);
        }
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        await Request.ExecuteAsync();
    }
}
