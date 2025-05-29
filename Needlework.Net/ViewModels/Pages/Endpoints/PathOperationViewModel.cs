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
    [ObservableProperty] private Lazy<RequestViewModel> _request;

    public PathOperationViewModel(PathOperation pathOperation, ILogger<RequestViewModel> requestViewModelLogger, Document document, Tab tab, System.Net.Http.HttpClient httpClient)
    {
        Path = pathOperation.Path;
        Operation = new OperationViewModel(pathOperation.Operation, document);
        Request = new(() => new RequestViewModel(requestViewModelLogger, tab, httpClient)
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

        Request.Value.RequestPath = sb.ToString();
        await Request.Value.ExecuteAsync();
    }

    [RelayCommand]
    private void CopyUrl()
    {
        App.MainWindow?.Clipboard?.SetTextAsync(Url);
    }
}
