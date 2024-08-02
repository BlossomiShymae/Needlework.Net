using Avalonia.Collections;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class ConsoleViewModel : PageBase
    {
        public IAvaloniaReadOnlyList<string> RequestMethods { get; } = new AvaloniaList<string>(["GET", "POST", "PUT", "DELETE", "HEAD", "PATCH", "OPTIONS", "TRACE"]);

        [ObservableProperty] private string? _requestMethodSelected = "GET";
        [ObservableProperty] private string? _requestPath = null;
        [ObservableProperty] private string? _requestBody = null;
        [ObservableProperty] private string? _responsePath = null;
        [ObservableProperty] private string? _responseStatus = null;
        [ObservableProperty] private string? _responseAuthentication = null;

        public event EventHandler<TextUpdatedEventArgs>? ResponseBodyUpdated;

        public ConsoleViewModel() : base("Console", Material.Icons.MaterialIconKind.Console, -100)
        {
        }

        [RelayCommand]
        private async Task SendRequest()
        {
            try
            {
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
                var response = await Connector.SendAsync(method, RequestPath) ?? throw new Exception("Response is null.");
                var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
                var body = await response.Content.ReadAsStringAsync();

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ResponseStatus = response.StatusCode.ToString();
                    ResponsePath = $"https://127.0.0.1/{processInfo.AppPort}{RequestPath}";
                    ResponseAuthentication = riotAuthentication.Value;
                    ResponseBodyUpdated?.Invoke(this, new(body));
                });
            }
            catch (Exception ex)
            {
                await SukiHost.ShowToast("Request Failed", ex.Message, SukiUI.Enums.NotificationType.Error);
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ResponseStatus = null;
                    ResponsePath = null;
                    ResponseAuthentication = null;
                    ResponseBodyUpdated?.Invoke(this, new(string.Empty));
                });
            }
        }
    }
}
