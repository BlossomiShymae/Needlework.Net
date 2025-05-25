using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PathOperationViewModel : ObservableObject
{
    public string Path { get; }
    public OperationViewModel Operation { get; }

    public string Url { get; }

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private Lazy<LcuRequestViewModel> _lcuRequest;

    public PathOperationViewModel(PathOperation pathOperation, ILogger<LcuRequestViewModel> lcuRequestViewModelLogger, Document lcuSchemaDocument)
    {
        Path = pathOperation.Path;
        Operation = new OperationViewModel(pathOperation.Operation, lcuSchemaDocument);
        LcuRequest = new(() => new LcuRequestViewModel(lcuRequestViewModelLogger)
        {
            Method = pathOperation.Method.ToUpper()
        });
        Url = $"https://swagger.dysolix.dev/lcu/#/{pathOperation.Tag}/{pathOperation.Operation.OperationId}";
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        var sb = new StringBuilder(Path);
        foreach (var pathParameter in Operation.PathParameters)
        {
            sb.Replace($"{{{pathParameter.Name}}}", pathParameter.Value);
        }

        var firstQueryAdded = false;
        foreach (var queryParameter in Operation.QueryParameters)
        {
            if (!string.IsNullOrWhiteSpace(queryParameter.Value))
            {
                sb.Append(firstQueryAdded ? '&' : '?');
                firstQueryAdded = true;
                sb.Append($"{queryParameter.Name}={Uri.EscapeDataString(queryParameter.Value)}");
            }
        }

        LcuRequest.Value.RequestPath = sb.ToString();
        await LcuRequest.Value.ExecuteAsync();
    }

    [RelayCommand]
    private void CopyUrl()
    {
        App.MainWindow?.Clipboard?.SetTextAsync(Url);
    }
}
