using Needlework.Net.Models;
using Needlework.Net.ViewModels.Shared;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PathOperationViewModel : ReactiveObject
{
    public PathOperationViewModel(PathOperation pathOperation, Document document, Tab tab)
    {
        Path = pathOperation.Path;
        Operation = new OperationViewModel(pathOperation.Operation, document);
        Request = new(() => new RequestViewModel(tab)
        {
            Method = pathOperation.Method.ToUpper()
        });
        Url = $"https://swagger.dysolix.dev/lcu/#/{Uri.EscapeDataString(pathOperation.Tag)}/{pathOperation.Operation.OperationId}";
        Markdown = $"[{pathOperation.Method.ToUpper()} {Path}]({Url})";
    }

    public string Path { get; }

    public OperationViewModel Operation { get; }

    public string Url { get; }

    public string Markdown { get; }

    [Reactive]
    private bool _isBusy;

    [Reactive]
    private Lazy<RequestViewModel> _request;

    [ReactiveCommand]
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

    [ReactiveCommand]
    private void CopyUrl()
    {
        App.MainWindow?.Clipboard?.SetTextAsync(Url);
    }

    [ReactiveCommand]
    private void CopyMarkdown()
    {
        App.MainWindow?.Clipboard?.SetTextAsync(Markdown);
    }
}
