using Avalonia.Media;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using Needlework.Net.Models;
using Needlework.Net.ViewModels.MainWindow;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class PathOperationViewModel : ObservableObject
{
    public string Method { get; }
    public SolidColorBrush Color { get; }
    public string Path { get; }
    public OperationViewModel Operation { get; }

    public ProcessInfo? ProcessInfo { get; }

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private Lazy<ResponseViewModel> _response;

    public PathOperationViewModel(PathOperation pathOperation)
    {
        Method = pathOperation.Method.ToUpper();
        Color = new SolidColorBrush(GetColor(Method));
        Path = pathOperation.Path;
        Operation = new OperationViewModel(pathOperation.Operation);
        Response = new(() => new ResponseViewModel(pathOperation.Path));
    }

    [RelayCommand]
    public async Task SendRequest()
    {
        try
        {
            IsBusy = true;

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
                _ => throw new Exception("Method is missing.")
            };

            var processInfo = ProcessFinder.Get();
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
            var uri = sb.ToString();

            var requestBody = WeakReferenceMessenger.Default.Send(new ContentRequestMessage(), "EndpointRequestEditor").Response;
            var content = new StringContent(requestBody, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

            var client = Connector.GetLcuHttpClientInstance();
            var response = await client.SendAsync(new(method, uri) { Content = content });
            var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
            var responseBytes = await response.Content.ReadAsByteArrayAsync();

            var responseBody = responseBytes.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBytes), App.JsonSerializerOptions) : string.Empty;
            if (responseBody.Length >= App.MaxCharacters)
            {
                WeakReferenceMessenger.Default.Send(new OopsiesDialogRequestedMessage(responseBody));
                WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(string.Empty, "EndpointResponseEditor")));
            }
            else WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(responseBody, "EndpointResponseEditor")));

            Response.Value.Status = $"{(int)response.StatusCode} {response.StatusCode}";
            Response.Value.Path = $"https://127.0.0.1:{processInfo.AppPort}{uri}";
            Response.Value.Authentication = Response.Value.Authorization = $"Basic {riotAuthentication.Value}";
            Response.Value.Username = riotAuthentication.Username;
            Response.Value.Password = riotAuthentication.Password;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new InfoBarUpdateMessage(new InfoBarViewModel("Request Failed", true, ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error, TimeSpan.FromSeconds(5))));
            WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(string.Empty, "EndpointResponseEditor")));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public static Color GetColor(string method) => method switch
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
