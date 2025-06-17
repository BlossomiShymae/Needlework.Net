using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using BlossomiShymae.Briar;
using BlossomiShymae.Briar.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Extensions;
using Needlework.Net.Messages;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Pages.Endpoints;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Shared;

public partial class RequestViewModel : ObservableObject, IEnableLogger
{
    private readonly NotificationService _notificationService;

    private readonly Tab _tab;

    public RequestViewModel(NotificationService notificationService, Tab tab)
    {
        _tab = tab;
        _notificationService = notificationService;
    }

    [ObservableProperty]
    private string? _method = "GET";

    [ObservableProperty]
    private SolidColorBrush _color = new(GetColor("GET"));

    [ObservableProperty]
    private bool _isRequestBusy;

    [ObservableProperty]
    private string? _requestPath;

    [ObservableProperty]
    private TextDocument _requestDocument = new();

    [ObservableProperty]
    private Vector _requestDocumentOffset = new();

    [ObservableProperty]
    private TextDocument _responseDocument = new();

    [ObservableProperty]
    private Vector _responseDocumentOffset = new();

    [ObservableProperty]
    private double _responseDocumentHorizontalScrollBar;

    [ObservableProperty]
    private double _responseDocumentVerticalScrollBar;

    [ObservableProperty]
    private string? _responsePath;

    [ObservableProperty]
    private string? _responseStatus;

    [ObservableProperty]
    private string? _responseAuthentication;

    [ObservableProperty]
    private string? _responseUsername;

    [ObservableProperty]
    private string? _responsePassword;

    [ObservableProperty]
    private string? _responseAuthorization;

    partial void OnMethodChanged(string? oldValue, string? newValue)
    {
        if (newValue == null) return;

        Color = new(GetColor(newValue));
    }

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
            var requestBody = RequestDocument.Text;
            var content = new StringContent(requestBody, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var client = Connector.GetGameHttpClientInstance();
            var response = await client.SendAsync(new HttpRequestMessage(method, RequestPath) { Content = content });
            var responseBody = await response.Content.ReadAsByteArrayAsync();

            var body = responseBody.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;
            if (body.Length > App.MaxCharacters)
            {
                WeakReferenceMessenger.Default.Send(new OopsiesDialogRequestedMessage(body));
                ResponseDocument = new();
            }
            else
            {
                ResponseDocument = new(body);
            }

            ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode.ToString()}";
            ResponsePath = $"https://127.0.0.1:2999{RequestPath}";

        }
        catch (Exception ex)
        {
            this.Log()
                .Error(ex, "Request failed: {Tuple}", (Method, RequestPath));
            _notificationService.Notify("Request failed", ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error);

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
            var requestBody = RequestDocument.Text;
            var content = new StringContent(requestBody, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var client = Connector.GetLcuHttpClientInstance();
            var response = await client.SendAsync(new(method, RequestPath) { Content = content });
            var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
            var responseBody = await response.Content.ReadAsByteArrayAsync();

            var body = responseBody.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;
            if (body.Length >= App.MaxCharacters)
            {
                WeakReferenceMessenger.Default.Send(new OopsiesDialogRequestedMessage(body));
                ResponseDocument = new();
            }
            else
            {
                ResponseDocument = new(body);
            }

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
            _notificationService.Notify("Request failed", ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error);

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

    private static Color GetColor(string method) => method switch
    {
        "GET" => Avalonia.Media.Color.FromRgb(95, 99, 186),
        "POST" => Avalonia.Media.Color.FromRgb(103, 186, 95),
        "PUT" => Avalonia.Media.Color.FromRgb(186, 139, 95),
        "DELETE" => Avalonia.Media.Color.FromRgb(186, 95, 95),
        "HEAD" => Avalonia.Media.Color.FromRgb(136, 95, 186),
        "PATCH" => Avalonia.Media.Color.FromRgb(95, 186, 139),
        _ => throw new InvalidOperationException("Method does not have assigned color.")
    };
}
