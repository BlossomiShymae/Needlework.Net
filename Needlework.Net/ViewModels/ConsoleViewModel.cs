using Avalonia.Collections;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Needlework.Net.Messages;
using Needlework.Net.Services;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels
{
    public partial class ConsoleViewModel : PageBase, IRecipient<DataReadyMessage>
    {
        public IAvaloniaReadOnlyList<string> RequestMethods { get; } = new AvaloniaList<string>(["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"]);
        public IAvaloniaList<string> RequestPaths { get; } = new AvaloniaList<string>();

        [ObservableProperty] private bool _isBusy = true;
        [ObservableProperty] private bool _isRequestBusy = false;
        [ObservableProperty] private string? _requestMethodSelected = "GET";
        [ObservableProperty] private string? _requestPath = null;
        [ObservableProperty] private string? _requestBody = null;
        [ObservableProperty] private string? _responsePath = null;
        [ObservableProperty] private string? _responseStatus = null;
        [ObservableProperty] private string? _responseAuthorization = null;

        public WindowService WindowService { get; }

        public ConsoleViewModel(WindowService windowService) : base("Console", "terminal", -200)
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
                var content = new StringContent(requestBody, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
                var response = await Connector.SendAsync(method, RequestPath, content);
                var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
                var responseBody = await response.Content.ReadAsByteArrayAsync();
  
                var body = responseBody.Length > 0 ? JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(responseBody), App.JsonSerializerOptions) : string.Empty;
                if (body.Length >= App.MaxCharacters)
                {
                    WindowService.ShowOopsiesWindow(body);
                    WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(string.Empty), nameof(ConsoleViewModel));
                }
                else WeakReferenceMessenger.Default.Send(new ResponseUpdatedMessage(body), nameof(ConsoleViewModel));
               
                ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode.ToString()}";
                ResponsePath = $"https://127.0.0.1:{processInfo.AppPort}{RequestPath}";
                ResponseAuthorization = $"Basic {riotAuthentication.Value}";
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new InfoBarUpdateMessage(new InfoBarViewModel("Request Failed", true, ex.Message, FluentAvalonia.UI.Controls.InfoBarSeverity.Error, TimeSpan.FromSeconds(5))));
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
                RequestPaths.Clear();
                RequestPaths.AddRange(message.Value.Paths);
                IsBusy = false;
            });
        }
    }
}
