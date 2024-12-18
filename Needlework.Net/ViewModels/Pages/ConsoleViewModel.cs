using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using Needlework.Net.Messages;
using Needlework.Net.ViewModels.Shared;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;

public partial class ConsoleViewModel : PageBase, IRecipient<DataReadyMessage>
{
    public IAvaloniaReadOnlyList<string> RequestMethods { get; } = new AvaloniaList<string>(["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"]);
    public IAvaloniaList<string> RequestPaths { get; } = new AvaloniaList<string>();

    [ObservableProperty] private bool _isBusy = true;
    [ObservableProperty] private LcuRequestViewModel _lcuRequest;

    public ConsoleViewModel(ILogger<LcuRequestViewModel> lcuRequestViewModelLogger) : base("Console", "terminal", -200)
    {
        _lcuRequest = new(lcuRequestViewModelLogger);
        WeakReferenceMessenger.Default.Register<DataReadyMessage>(this);
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        await LcuRequest.ExecuteAsync();
    }

    public void Receive(DataReadyMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
        {
            RequestPaths.Clear();
            RequestPaths.AddRange(message.Value.Paths);
            IsBusy = false;
        });
    }
}
