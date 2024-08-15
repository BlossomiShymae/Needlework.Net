using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Microsoft.OpenApi.Models;
using Needlework.Net.Core;
using Needlework.Net.Desktop.Messages;
using Needlework.Net.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject, IRecipient<DataRequestMessage>, IRecipient<HostDocumentRequestMessage>, IRecipient<OopsiesWindowRequestedMessage>, IRecipient<InfoBarUpdateMessage>
    {
        public IAvaloniaReadOnlyList<NavigationViewItem> MenuItems { get; }
        [NotifyPropertyChangedFor(nameof(CurrentPage))]
        [ObservableProperty] private NavigationViewItem _selectedMenuItem;
        public PageBase CurrentPage => (PageBase)SelectedMenuItem.Tag!;

        public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";
        [ObservableProperty] private bool _isUpdateShown = false;

        public HttpClient HttpClient { get; }
        public WindowService WindowService { get; }
        public LcuSchemaHandler? LcuSchemaHandler { get; set; }
        public OpenApiDocument? HostDocument { get; set; }

        [ObservableProperty] private bool _isBusy = true;

        [ObservableProperty] private ObservableCollection<InfoBarViewModel> _infoBarItems = [];

        public MainWindowViewModel(IEnumerable<PageBase> pages, HttpClient httpClient, WindowService windowService)
        {
            MenuItems = new AvaloniaList<NavigationViewItem>(pages
                .OrderBy(p => p.Index)
                .ThenBy(p => p.DisplayName)
                .Select(p => new NavigationViewItem()
                {
                    Content = p.DisplayName,
                    Tag = p,
                    IconSource = new BitmapIconSource() { UriSource = new Uri($"avares://NeedleworkDotNet/Assets/Icons/{p.Icon}.png") }
                }));
            SelectedMenuItem = MenuItems[0];

            HttpClient = httpClient;
            WindowService = windowService;

            WeakReferenceMessenger.Default.RegisterAll(this);

            Task.Run(FetchDataAsync);
            new Thread(ProcessEvents) { IsBackground = true }.Start();
        }

        private void ProcessEvents(object? obj)
        {
            while (true)
            {
                Task.Run(CheckLatestVersionAsync);

                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }

        private async Task CheckLatestVersionAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/BlossomiShymae/Needlework.Net/releases/latest");
                request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Needlework.Net", Version));

                var response = await HttpClient.SendAsync(request);
                var release = await response.Content.ReadFromJsonAsync<GithubRelease>();
                if (release == null) return;

                var currentVersion = int.Parse(Version.Replace(".", ""));

                if (release.IsLatest(currentVersion) && !IsUpdateShown)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                    {
                        await ShowInfoBarAsync(new("Needlework.Net Update", true, $"There is a new version available: {release.TagName}.", InfoBarSeverity.Informational, TimeSpan.FromSeconds(10), new Avalonia.Controls.Button()
                        {
                            Command = OpenUrlCommand,
                            CommandParameter = "https://github.com/BlossomiShymae/Needlework.Net/releases",
                            Content = "Download"
                        }));
                        IsUpdateShown = true;
                    });
                }
            }
            catch (Exception) { }
        }

        private async Task FetchDataAsync()
        {
            var document = await Resources.GetOpenApiDocumentAsync(HttpClient);
            HostDocument = document;
            var handler = new LcuSchemaHandler(document);
            LcuSchemaHandler = handler;

            WeakReferenceMessenger.Default.Send(new DataReadyMessage(handler));
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

        public void Receive(OopsiesWindowRequestedMessage message)
        {
            WindowService.ShowOopsiesWindow(message.Value);
        }

        public void Receive(InfoBarUpdateMessage message)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(async () => await ShowInfoBarAsync(message.Value));
        }

        private async Task ShowInfoBarAsync(InfoBarViewModel vm)
        {
            InfoBarItems.Add(vm);
            await Task.Delay(vm.Duration);
            InfoBarItems.Remove(vm);
        }
    }
}
