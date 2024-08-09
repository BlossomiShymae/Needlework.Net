using Avalonia.Collections;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Desktop.Messages;
using Needlework.Net.Desktop.Services;
using SukiUI.Controls;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class ConsoleViewModel : PageBase, IRecipient<DataReadyMessage>
    {
        public IAvaloniaReadOnlyList<string> RequestMethods { get; } = new AvaloniaList<string>(["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"]);

        [ObservableProperty] private bool _isBusy = true;
        [ObservableProperty] private bool _isRequestBusy = false;
        [ObservableProperty] private IAvaloniaReadOnlyList<string> _requestPaths = new AvaloniaList<string>();
        [ObservableProperty] private string? _requestMethodSelected = "GET";
        [ObservableProperty] private string? _requestPath = null;
        [ObservableProperty] private string? _requestBody = null;
        [ObservableProperty] private string? _responsePath = null;
        [ObservableProperty] private string? _responseStatus = null;
        [ObservableProperty] private string? _responseAuthorization = null;

        public WindowService WindowService { get; }

        public ConsoleViewModel(WindowService windowService) : base("Console", Material.Icons.MaterialIconKind.Console, -200)
        {
            WindowService = windowService;

            WeakReferenceMessenger.Default.Register<DataReadyMessage>(this);
        }

        [RelayCommand]
        private async Task SendRequest()
        {
            try
            {
                IsRequestBusy = true;
                if (string.IsNullOrEmpty(RequestPath)) throw new Exception("Path is empty.");

                var method = RequestMethodSelected switch
                {
                    "GET" => HttpMethod.Get,
                    "POST" => HttpMethod.Post,
                    "PUT" => HttpMethod.Put,
                    "DELETE" => HttpMethod.Delete,
                    "HEAD" => HttpMethod.Head,
                    "PATCH" => HttpMethod.Patch,
                    "OPTIONS" => HttpMethod.Options,
                    "TRACE" => HttpMethod.Trace,
                    _ => throw new Exception("Method is not selected."),
                };

                var processInfo = Connector.GetProcessInfo();
                var requestBody = WeakReferenceMessenger.Default.Send(new ContentRequestMessage(), "ConsoleRequestEditor").Response;
                var content = new StringContent(Regex.Replace(requestBody, @"\s+", ""), new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
                var response = await Connector.SendAsync(method, RequestPath, content) ?? throw new Exception("Response is null.");
                var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
                var body = await response.Content.ReadAsStringAsync();

                body = !string.IsNullOrEmpty(body) ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(body), App.JsonSerializerOptions) : string.Empty;
                if (body.Length >= App.MaxCharacters) WindowService.ShowOopsiesWindow(body);
                else WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(body), nameof(ConsoleViewModel));

                ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode.ToString()}";
                ResponsePath = $"https://127.0.0.1:{processInfo.AppPort}{RequestPath}";
                ResponseAuthorization = $"Basic {riotAuthentication.Value}";
            }
            catch (Exception ex)
            {
                await SukiHost.ShowToast("Request Failed", ex.Message, SukiUI.Enums.NotificationType.Error);
                ResponseStatus = null;
                ResponsePath = null;
                ResponseAuthorization = null;
                WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(string.Empty), nameof(ConsoleViewModel));
            }
            finally
            {
                IsRequestBusy = false;
            }
        }

        public void Receive(DataReadyMessage message)
        {
            Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
            {
                RequestPaths = new AvaloniaList<string>([.. message.Value.Paths]);
                IsBusy = false;
            });
        }
    }
}
