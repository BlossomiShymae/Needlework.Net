using Avalonia.Media;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using Needlework.Net.ViewModels.MainWindow;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Shared;

public partial class LcuRequestViewModel : ObservableObject
{
    [ObservableProperty] private string? _method = "GET";
    [ObservableProperty] private SolidColorBrush _color = new(GetColor("GET"));

    [ObservableProperty] private bool _isRequestBusy = false;
    [ObservableProperty] private string? _requestPath = null;
    [ObservableProperty] private string? _requestBody = null;

    [ObservableProperty] private string? _responsePath = null;
    [ObservableProperty] private string? _responseStatus = null;
    [ObservableProperty] private string? _responseAuthentication = null;
    [ObservableProperty] private string? _responseUsername = null;
    [ObservableProperty] private string? _responsePassword = null;
    [ObservableProperty] private string? _responseAuthorization = null;
    [ObservableProperty] private string? _responseBody = null;

    public event EventHandler<LcuRequestViewModel>? RequestText;
    public event EventHandler<string>? UpdateText;

    partial void OnMethodChanged(string? oldValue, string? newValue)
    {
        if (newValue == null) return;

        Color = new(GetColor(newValue));
    }

    public async Task ExecuteAsync()
    {
        try
        {
            IsRequestBusy = true;
            if (string.IsNullOrEmpty(RequestPath))
                throw new Exception("Path is empty.");

            var method = Method switch
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

            var processInfo = ProcessFinder.Get();
            RequestText?.Invoke(this, this);
            var content = new StringContent(RequestBody ?? string.Empty, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            var client = Connector.GetLcuHttpClientInstance();
            var response = await client.SendAsync(new(method, RequestPath) { Content = content });
            var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
            var responseBody = await response.Content.ReadAsByteArrayAsync();

            var body = responseBody.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;
            if (body.Length >= App.MaxCharacters)
            {
                WeakReferenceMessenger.Default.Send(new OopsiesDialogRequestedMessage(body));
                UpdateText?.Invoke(this, string.Empty);
            }
            else
            {
                ResponseBody = body;
                UpdateText?.Invoke(this, body);
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
            WeakReferenceMessenger.Default.Send(new InfoBarUpdateMessage(new InfoBarViewModel("Request Failed", true, ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error, TimeSpan.FromSeconds(5))));
            UpdateText?.Invoke(this, string.Empty);

            ResponseStatus = null;
            ResponsePath = null;
            ResponseAuthentication = null;
            ResponseAuthorization = null;
            ResponseUsername = null;
            ResponsePassword = null;
            ResponseBody = null;
        }
        finally
        {
            IsRequestBusy = false;
        }
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
