using Avalonia.Collections;
using BlossomiShymae.GrrrLCU;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
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
using System.Threading.Tasks;
using System.Timers;

namespace Needlework.Net.ViewModels.MainWindow;

public partial class MainWindowViewModel
    : ObservableObject, IRecipient<InfoBarUpdateMessage>, IRecipient<OopsiesDialogRequestedMessage>
{
    public IAvaloniaReadOnlyList<NavigationViewItem> MenuItems { get; }
    [ObservableProperty] private NavigationViewItem _selectedMenuItem;
    [ObservableProperty] private PageBase _currentPage;

    public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";
    [ObservableProperty] private bool _isUpdateShown = false;

    [ObservableProperty] private string _schemaVersion = "N/A";
    [ObservableProperty] private string _schemaVersionLatest = "N/A";

    public HttpClient HttpClient { get; }
    public DialogService DialogService { get; }

    private readonly DataSource _dataSource;

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

    public MainWindowViewModel(IEnumerable<PageBase> pages, HttpClient httpClient, DialogService dialogService, ILogger<MainWindowViewModel> logger, DataSource dataSource)
    {
        _logger = logger;
        _dataSource = dataSource;

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
        CurrentPage = (PageBase)MenuItems[0].Tag!;

        HttpClient = httpClient;
        DialogService = dialogService;

        WeakReferenceMessenger.Default.RegisterAll(this);

        _latestUpdateTimer.Elapsed += OnLatestUpdateTimerElapsed;
        _schemaVersionTimer.Elapsed += OnSchemaVersionTimerElapsed;
        _latestUpdateTimer.Start();
        _schemaVersionTimer.Start();
        OnLatestUpdateTimerElapsed(null, null);
        OnSchemaVersionTimerElapsed(null, null);

    }

    partial void OnSelectedMenuItemChanged(NavigationViewItem value)
    {
        if (value.Tag is PageBase page)
        {
            CurrentPage = page;
            if (!page.IsInitialized)
            {
                Task.Run(page.InitializeAsync);
            }
        }
    }

    private async void OnSchemaVersionTimerElapsed(object? sender, ElapsedEventArgs? e)
    {
        if (!ProcessFinder.IsPortOpen()) return;
        var lcuSchemaDocument = await _dataSource.GetLcuSchemaDocumentAsync();

        try
        {
            var client = Connector.GetLcuHttpClientInstance();

            var currentSemVer = lcuSchemaDocument.Info.Version.Split('.');
            var systemBuild = await client.GetFromJsonAsync<SystemBuild>("/system/v1/builds") ?? throw new NullReferenceException();
            var latestSemVer = systemBuild.Version.Split('.');

            if (!_isSchemaVersionChecked)
            {
                _logger.LogInformation("LCU Schema (current): {Version}", lcuSchemaDocument.Info.Version);
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
