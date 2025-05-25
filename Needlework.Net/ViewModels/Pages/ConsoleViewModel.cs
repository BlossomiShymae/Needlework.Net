using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Needlework.Net.ViewModels.Shared;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;

public partial class ConsoleViewModel : PageBase
{
    public IAvaloniaReadOnlyList<string> RequestMethods { get; } = new AvaloniaList<string>(["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"]);
    public IAvaloniaList<string> RequestPaths { get; } = new AvaloniaList<string>();

    [ObservableProperty] private bool _isBusy = true;
    [ObservableProperty] private LcuRequestViewModel _lcuRequest;

    private readonly DataSource _dataSource;

    public ConsoleViewModel(ILogger<LcuRequestViewModel> lcuRequestViewModelLogger, DataSource dataSource) : base("Console", "terminal", -200)
    {
        _lcuRequest = new(lcuRequestViewModelLogger);
        _dataSource = dataSource;
    }

    public override async Task InitializeAsync()
    {
        var document = await _dataSource.GetLolClientDocumentAsync();
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
        await LcuRequest.ExecuteAsync();
    }
}
