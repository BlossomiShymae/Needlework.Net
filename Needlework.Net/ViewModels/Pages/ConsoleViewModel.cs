using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Needlework.Net.ViewModels.Shared;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;

public partial class ConsoleViewModel : PageBase
{
    public IAvaloniaReadOnlyList<string> RequestMethods { get; } = new AvaloniaList<string>(["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"]);
    public IAvaloniaList<string> RequestPaths { get; } = new AvaloniaList<string>();

    [ObservableProperty] private bool _isBusy = true;
    [ObservableProperty] private RequestViewModel _request;

    private readonly DataSource _dataSource;

    public ConsoleViewModel(ILogger<RequestViewModel> requestViewModelLogger, DataSource dataSource, HttpClient httpClient) : base("Console", "terminal", -200)
    {
        _request = new(requestViewModelLogger, Endpoints.Tab.LCU, httpClient);
        _dataSource = dataSource;
    }

    public override async Task InitializeAsync()
    {
        var document = await _dataSource.GetLcuSchemaDocumentAsync();
        Dispatcher.UIThread.Invoke(() =>
        {
            RequestPaths.Clear();
            RequestPaths.AddRange(document.Paths);
        });
        IsBusy = false;
        IsInitialized = true;
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        await Request.ExecuteAsync();
    }
}
