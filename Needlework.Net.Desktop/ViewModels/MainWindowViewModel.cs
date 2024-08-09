using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.OpenApi.Models;
using Needlework.Net.Core;
using Needlework.Net.Desktop.Messages;
using Needlework.Net.Desktop.Services;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject, IRecipient<DataRequestMessage>, IRecipient<HostDocumentRequestMessage>, IRecipient<OopsiesWindowRequestedMessage>
    {
        public IAvaloniaReadOnlyList<PageBase> Pages { get; }
        public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";

        public HttpClient HttpClient { get; }
        public WindowService WindowService { get; }
        public LcuSchemaHandler? LcuSchemaHandler { get; set; }
        public OpenApiDocument? HostDocument { get; set; }

        [ObservableProperty] private bool _isBusy = true;

        public MainWindowViewModel(IEnumerable<PageBase> pages, HttpClient httpClient, WindowService windowService)
        {
            Pages = new AvaloniaList<PageBase>(pages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
            HttpClient = httpClient;
            WindowService = windowService;

            WeakReferenceMessenger.Default.RegisterAll(this);
            Task.Run(FetchDataAsync);
        }

        private async Task FetchDataAsync()
        {
            var document = await Resources.GetOpenApiDocumentAsync(HttpClient);
            HostDocument = document;
            var handler = new LcuSchemaHandler(document);
            LcuSchemaHandler = handler;

            WeakReferenceMessenger.Default.Send(new DataReadyMessage(handler));
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => await SukiHost.ShowToast("OpenAPI Data Processed", "Some pages can now be used.", SukiUI.Enums.NotificationType.Success, TimeSpan.FromSeconds(5)));
            IsBusy = false;
        }

        public void Receive(DataRequestMessage message)
        {
            message.Reply(LcuSchemaHandler!);
        }

        public void Receive(HostDocumentRequestMessage message)
        {
            message.Reply(HostDocument!);
        }

        [RelayCommand]
        private void OpenUrl(string url)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                }
            };
            process.Start();
        }

        [RelayCommand]
        private void OpenConsole()
        {

        }

        public void Receive(OopsiesWindowRequestedMessage message)
        {
            WindowService.ShowOopsiesWindow(message.Value);
        }
    }
}
