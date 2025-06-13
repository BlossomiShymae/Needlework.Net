using Avalonia.Media;
using AvaloniaEdit.Document;
using BlossomiShymae.Briar;
using BlossomiShymae.Briar.Utils;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Pages.Endpoints;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Shared;

public partial class RequestViewModel : ReactiveObject, IEnableLogger
{
    private readonly NotificationService _notificationService;

    private readonly Tab _tab;

    public RequestViewModel(Pages.Endpoints.Tab tab, NotificationService? notificationService = null)
    {
        _tab = tab;
        _notificationService = notificationService ?? Locator.Current.GetService<NotificationService>()!;

        _colorHelper = this.WhenAnyValue(x => x.Method)
            .Select(method => GetSolidCrushBrush(method ?? "GET"))
            .ToProperty(this, x => x.Color);
    }

    [ObservableAsProperty]
    private SolidColorBrush _color = GetSolidCrushBrush("GET");

    [Reactive]
    private string? _method = "GET";

    [Reactive]
    private bool _isRequestBusy;

    [Reactive]
    private string? _requestPath;

    [Reactive]
    private TextDocument _requestDocument = new();

    [Reactive]
    private string? _responsePath;

    [Reactive]
    private string? _responseStatus;

    [Reactive]
    private string? _responseAuthentication;

    [Reactive]
    private string? _responseUsername;

    [Reactive]
    private string? _responsePassword;

    [Reactive]
    private string? _responseAuthorization;

    [Reactive]
    private TextDocument _responseDocument = new();

    public async Task ExecuteAsync()
    {
        switch (_tab)
        {
            case Tab.LCU:
                await ExecuteLcuAsync();
                break;
            case Tab.GameClient:
                await ExecuteGameClientAsync();
                break;
            default:
                break;
        }
    }

    private async Task ExecuteGameClientAsync()
    {
        try
        {
            IsRequestBusy = true;
            if (string.IsNullOrEmpty(RequestPath))
                throw new Exception("Path is empty.");
            var method = GetMethod();

            this.Log()
                .Debug("Sending request: {Tuple}", (Method, RequestPath));

            var content = new StringContent(RequestDocument.Text, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var client = Connector.GetGameHttpClientInstance();
            var response = await client.SendAsync(new HttpRequestMessage(method, RequestPath) { Content = content });
            var responseBody = await response.Content.ReadAsByteArrayAsync();
            var body = responseBody.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;

            ResponseDocument = new(body);
            ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode.ToString()}";
            ResponsePath = $"https://127.0.0.1:2999{RequestPath}";

        }
        catch (Exception ex)
        {
            this.Log()
                .Error(ex, "Request failed: {Tuple}", (Method, RequestPath));
            _notificationService.Notify("Request Failed", ex.Message, InfoBarSeverity.Error);

            ResponseStatus = null;
            ResponsePath = null;
            ResponseAuthentication = null;
            ResponseAuthorization = null;
            ResponseUsername = null;
            ResponsePassword = null;
            ResponseDocument = new();
        }
        finally
        {
            IsRequestBusy = false;
        }
    }

    private async Task ExecuteLcuAsync()
    {
        try
        {
            IsRequestBusy = true;
            if (string.IsNullOrEmpty(RequestPath))
                throw new Exception("Path is empty.");
            var method = GetMethod();

            this.Log()
                .Debug("Sending request: {Tuple}", (Method, RequestPath));

            var processInfo = ProcessFinder.GetProcessInfo();
            var content = new StringContent(RequestDocument.Text, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var client = Connector.GetLcuHttpClientInstance();
            var response = await client.SendAsync(new(method, RequestPath) { Content = content });
            var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
            var responseBody = await response.Content.ReadAsByteArrayAsync();
            var body = responseBody.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;

            ResponseDocument = new(body);
            ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode.ToString()}";
            ResponsePath = $"https://127.0.0.1:{processInfo.AppPort}{RequestPath}";
            ResponseAuthentication = riotAuthentication.Value;
            ResponseAuthorization = $"Basic {riotAuthentication.Value}";
            ResponseUsername = riotAuthentication.Username;
            ResponsePassword = riotAuthentication.Password;
        }
        catch (Exception ex)
        {
            this.Log()
                .Error(ex, "Request failed: {Tuple}", (Method, RequestPath));
            _notificationService.Notify("Request Failed", ex.Message, InfoBarSeverity.Error);

            ResponseStatus = null;
            ResponsePath = null;
            ResponseAuthentication = null;
            ResponseAuthorization = null;
            ResponseUsername = null;
            ResponsePassword = null;
            ResponseDocument = new();
        }
        finally
        {
            IsRequestBusy = false;
        }
    }

    private HttpMethod GetMethod()
    {
        return Method switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "HEAD" => HttpMethod.Head,
            "PATCH" => HttpMethod.Patch,
            "OPTIONS" => HttpMethod.Options,
            "TRACE" => HttpMethod.Trace,
            _ => throw new Exception("Method is not selected or missing."),
        };
    }

    private static SolidColorBrush GetSolidCrushBrush(string? method = null) => new(method switch
    {
        "GET" or null => Avalonia.Media.Color.FromRgb(95, 99, 186),
        "POST" => Avalonia.Media.Color.FromRgb(103, 186, 95),
        "PUT" => Avalonia.Media.Color.FromRgb(186, 139, 95),
        "DELETE" => Avalonia.Media.Color.FromRgb(186, 95, 95),
        "HEAD" => Avalonia.Media.Color.FromRgb(136, 95, 186),
        "PATCH" => Avalonia.Media.Color.FromRgb(95, 186, 139),
        _ => throw new InvalidOperationException("Method does not have assigned color.")
    });
}
