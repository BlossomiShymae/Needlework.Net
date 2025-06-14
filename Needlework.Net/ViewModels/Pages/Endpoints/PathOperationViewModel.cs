using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PathOperationViewModel : ObservableObject
{
    public PathOperationViewModel(Services.NotificationService notificationService, PathOperation pathOperation, Document document, Tab tab)
    {
        Path = pathOperation.Path;
        Operation = new OperationViewModel(pathOperation.Operation, document);
        Request = new(() => new RequestViewModel(notificationService, tab)
        {
            Method = pathOperation.Method.ToUpper(),
            RequestDocument = new(Operation.RequestTemplate ?? string.Empty)
        });
        Url = $"https://swagger.dysolix.dev/lcu/#/{Uri.EscapeDataString(pathOperation.Tag)}/{pathOperation.Operation.OperationId}";
        Markdown = $"[{pathOperation.Method.ToUpper()} {Path}]({Url})";
    }

    public string Path { get; }

    public OperationViewModel Operation { get; }

    public string Url { get; }

    public string Markdown { get; }

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private Lazy<RequestViewModel> _request;

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

    [RelayCommand]
    private void CopyMarkdown()
    {
        App.MainWindow?.Clipboard?.SetTextAsync(Markdown);
    }
}
