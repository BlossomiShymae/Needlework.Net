using Avalonia.Media;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Core;
using Needlework.Net.Desktop.Messages;
using SukiUI.Controls;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class PathOperationViewModel : ObservableObject
    {
        public string Method { get; }
        public SolidColorBrush Color { get; }
        public string Path { get; }
        public OperationViewModel Operation { get; }
        public ProcessInfo? ProcessInfo { get; }

        [ObservableProperty] private bool _isBusy;
        [ObservableProperty] private string? _responsePath;
        [ObservableProperty] private string? _responseStatus;
        [ObservableProperty] private string? _responseAuthentication;
        [ObservableProperty] private string? _responseUsername;
        [ObservableProperty] private string? _responsePassword;
        [ObservableProperty] private string? _responseAuthorization;

        public PathOperationViewModel(PathOperation pathOperation)
        {
            Method = pathOperation.Method.ToUpper();
            Color = new SolidColorBrush(GetColor(pathOperation.Method.ToUpper()));
            Path = pathOperation.Path;
            Operation = new OperationViewModel(pathOperation.Operation);
            ProcessInfo = GetProcessInfo();
            ResponsePath = ProcessInfo != null ? $"https://127.0.0.1:{ProcessInfo.AppPort}{Path}" : null;
            ResponseUsername = ProcessInfo != null ? new RiotAuthentication(ProcessInfo.RemotingAuthToken).Username : null;
            ResponsePassword = ProcessInfo != null ? new RiotAuthentication(ProcessInfo.RemotingAuthToken).Password : null;
            ResponseAuthorization = ProcessInfo != null ? $"Basic {new RiotAuthentication(ProcessInfo.RemotingAuthToken).Value}" : null;
        }

        private ProcessInfo? GetProcessInfo()
        {
            try
            {
                var processInfo = Connector.GetProcessInfo();
                return processInfo;
            }
            catch (Exception) { }
            return null;
        }

        [RelayCommand]
        public async Task SendRequest()
        {
            try
            {
                IsBusy = true;

                var method = Method.ToUpper() switch
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

                var processInfo = Connector.GetProcessInfo();
                var path = Path;
                foreach (var pathParameter in Operation.PathParameters)
                {
                    path = path.Replace($"{{{pathParameter.Name}}}", pathParameter.Value);
                }

                var query = "";
                foreach (var queryParameter in Operation.QueryParameters)
                {
                    if (query.Length != 0 && !string.IsNullOrWhiteSpace(queryParameter.Value))
                        query += $"&{queryParameter.Name}={Uri.EscapeDataString(queryParameter.Value)}";
                    else if (query.Length == 0 && !string.IsNullOrWhiteSpace(queryParameter.Value))
                        query += $"?{queryParameter.Name}={Uri.EscapeDataString(queryParameter.Value)}";
                }
                var uri = $"{path}{query}";

                var requestBody = WeakReferenceMessenger.Default.Send(new ContentRequestMessage(), "EndpointRequestEditor").Response;
                var content = new StringContent(Regex.Replace(requestBody, @"\s+", ""), new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));

                var response = await Connector.SendAsync(method, $"{uri}", content) ?? throw new Exception("Response is null.");
                var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
                var responseBody = await response.Content.ReadAsStringAsync();

                responseBody = !string.IsNullOrEmpty(responseBody) ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;
                if (responseBody.Length >= App.MaxCharacters)
                {
                    WeakReferenceMessenger.Default.Send(new OopsiesWindowRequestedMessage(responseBody));
                    WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(string.Empty, "EndpointResponseEditor")));
                }
                else WeakReferenceMessenger.Default.Send(new EditorUpdateMessage(new(responseBody, "EndpointResponseEditor")));

                ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode}";
                ResponsePath = $"https://127.0.0.1:{processInfo.AppPort}{uri}";
                ResponseAuthentication = $"Basic {riotAuthentication.Value}";
                ResponseUsername = riotAuthentication.Username;
                ResponsePassword = riotAuthentication.Password;
            }
            catch (Exception ex)
            {
                await SukiHost.ShowToast("Request Failed", ex.Message, SukiUI.Enums.NotificationType.Error);
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
}
