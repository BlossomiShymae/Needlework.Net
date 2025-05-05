using Avalonia.Collections;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Needlework.Net.Messages;
using Needlework.Net.Models;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Pages;
using Needlework.Net.Views.MainWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Needlework.Net.ViewModels.MainWindow;

public partial class MainWindowViewModel
    : ObservableObject, IRecipient<DataRequestMessage>, IRecipient<HostDocumentRequestMessage>, IRecipient<InfoBarUpdateMessage>, IRecipient<OopsiesDialogRequestedMessage>
{
    public IAvaloniaReadOnlyList<NavigationViewItem> MenuItems { get; }
    [NotifyPropertyChangedFor(nameof(CurrentPage))]
    [ObservableProperty] private NavigationViewItem _selectedMenuItem;
    public PageBase CurrentPage => (PageBase)SelectedMenuItem.Tag!;

    public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";
    [ObservableProperty] private bool _isUpdateShown = false;

    [ObservableProperty] private string _schemaVersion = "N/A";
    [ObservableProperty] private string _schemaVersionLatest = "N/A";

    public HttpClient HttpClient { get; }
    public DialogService DialogService { get; }
    public OpenApiDocumentWrapper? OpenApiDocumentWrapper { get; set; }
    public OpenApiDocument? HostDocument { get; set; }

    [ObservableProperty] private bool _isBusy = true;

    [ObservableProperty] private ObservableCollection<InfoBarViewModel> _infoBarItems = [];

    private readonly ILogger<MainWindowViewModel> _logger;

    private readonly System.Timers.Timer _latestUpdateTimer = new()
    {
        Interval = TimeSpan.FromMinutes(10).TotalMilliseconds,
        Enabled = true
    };

    private readonly System.Timers.Timer _schemaVersionTimer = new()
    {
        Interval = TimeSpan.FromSeconds(5).TotalMilliseconds,
        Enabled = true
    };
    private bool _isSchemaVersionChecked = false;

    public MainWindowViewModel(IEnumerable<PageBase> pages, HttpClient httpClient, DialogService dialogService, ILogger<MainWindowViewModel> logger)
    {
        _logger = logger;

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
        DialogService = dialogService;

        WeakReferenceMessenger.Default.RegisterAll(this);

        Task.Run(FetchDataAsync);

        _latestUpdateTimer.Elapsed += OnLatestUpdateTimerElapsed;
        _schemaVersionTimer.Elapsed += OnSchemaVersionTimerElapsed;
        _latestUpdateTimer.Start();
        _schemaVersionTimer.Start();
        OnLatestUpdateTimerElapsed(null, null);
        OnSchemaVersionTimerElapsed(null, null);

    }

    private async void OnSchemaVersionTimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        if (OpenApiDocumentWrapper == null) return;
        if (!ProcessFinder.IsPortOpen()) return;

        try
        {
            var client = Connector.GetLcuHttpClientInstance();

            var currentSemVer = OpenApiDocumentWrapper.Info.Version.Split('.');
            var systemBuild = await client.GetFromJsonAsync<SystemBuild>("/system/v1/builds") ?? throw new NullReferenceException();
            var latestSemVer = systemBuild.Version.Split('.');

            if (!_isSchemaVersionChecked)
            {
                _logger.LogInformation("LCU Schema (current): {Version}", OpenApiDocumentWrapper.Info.Version);
                _logger.LogInformation("LCU Schema (latest): {Version}", systemBuild.Version);
                _isSchemaVersionChecked = true;
            }

            bool isVersionMatching = currentSemVer[0] == latestSemVer[0] && currentSemVer[1] == latestSemVer[1]; // Compare major and minor versions
            if (!isVersionMatching)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    await ShowInfoBarAsync(new("Newer System Build", true, $"LCU Schema is possibly outdated compared to latest system build. Consider submitting a pull request on dysolix/hasagi-types.\nCurrent: {string.Join(".", currentSemVer)}\nLatest: {string.Join(".", latestSemVer)}", InfoBarSeverity.Warning, TimeSpan.FromSeconds(60), new Avalonia.Controls.Button()
                    {
                        Command = OpenUrlCommand,
                        CommandParameter = "https://github.com/dysolix/hasagi-types#updating-the-types",
                        Content = "Submit PR"
                    }));
                });

                _schemaVersionTimer.Elapsed -= OnSchemaVersionTimerElapsed;
                _schemaVersionTimer.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema version check failed");
        }
    }

    private async void OnLatestUpdateTimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/BlossomiShymae/Needlework.Net/releases/latest");
            request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("Needlework.Net", Version));

            var response = await HttpClient.SendAsync(request);
            var release = await response.Content.ReadFromJsonAsync<GithubRelease>();
            if (release == null)
            {
                _logger.LogWarning("Release response is null");
                return;
            }

            var currentVersion = int.Parse(Version.Replace(".", ""));

            if (release.IsLatest(currentVersion))
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                {
                    await ShowInfoBarAsync(new("Needlework.Net Update", true, $"There is a new version available: {release.TagName}.", InfoBarSeverity.Informational, TimeSpan.FromSeconds(30), new Avalonia.Controls.Button()
                    {
                        Command = OpenUrlCommand,
                        CommandParameter = "https://github.com/BlossomiShymae/Needlework.Net/releases",
                        Content = "Download"
                    }));
                });

                _latestUpdateTimer.Elapsed -= OnLatestUpdateTimerElapsed;
                _latestUpdateTimer.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for latest version");
        }
    }

    private async Task FetchDataAsync()
    {
        try
        {
            var document = await Resources.GetOpenApiDocumentAsync(HttpClient);
            HostDocument = document;
            var handler = new OpenApiDocumentWrapper(document);
            OpenApiDocumentWrapper = handler;

            WeakReferenceMessenger.Default.Send(new DataReadyMessage(handler));
            IsBusy = false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch OpenAPI data");
        }
    }

    public void Receive(DataRequestMessage message)
    {
        message.Reply(OpenApiDocumentWrapper!);
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

    public void Receive(OopsiesDialogRequestedMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Invoke(async () => await DialogService.ShowAsync<OopsiesDialog>(message.Value));
    }
}
