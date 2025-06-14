using Avalonia;
using Avalonia.Media;
using BlossomiShymae.Briar;
using BlossomiShymae.Briar.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using Flurl.Http;
using Flurl.Http.Configuration;
using Needlework.Net.Extensions;
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
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.MainWindow;

public partial class MainWindowViewModel
    : ObservableObject, IRecipient<OopsiesDialogRequestedMessage>, IEnableLogger
{
    private readonly DocumentService _documentService;

    private readonly IFlurlClient _githubClient;

    private readonly NotificationService _notificationService;

    private readonly DialogService _dialogService;

    private readonly IDisposable _checkForUpdatesDisposable;

    private readonly IDisposable _checkForSchemaVersionDisposable;

    public MainWindowViewModel(IEnumerable<PageBase> pages, DialogService dialogService, DocumentService documentService, NotificationService notificationService, IFlurlClientCache clients)
    {
        _dialogService = dialogService;
        _documentService = documentService;
        _notificationService = notificationService;
        _githubClient = clients.Get("GithubClient");

        NavigationViewItems = pages
            .OrderBy(p => p.Index)
            .ThenBy(p => p.DisplayName)
            .Select(ToNavigationViewItem)
            .ToList();
        SelectedNavigationViewItem = NavigationViewItems.First();
        CurrentPage = (PageBase)SelectedNavigationViewItem.Tag!;

        _notificationService.Notifications.Subscribe(async notification =>
        {
            var vm = new NotificationViewModel(notification);
            Notifications.Add(vm);
            await Task.Delay(notification.Duration ?? TimeSpan.FromSeconds(10));
            Notifications.Remove(vm);
        });

        _checkForUpdatesDisposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(10))
            .Select(time => Unit.Default)
            .Subscribe(async _ =>
            {
                try
                {
                    await CheckForUpdatesAsync();
                }
                catch (Exception ex)
                {
                    var message = "Failed to check for updates. Please check your internet connection or try again later.";
                    this.Log()
                        .Error(ex, message);
                    _notificationService.Notify("Needlework.Net", message, InfoBarSeverity.Error);
                    _checkForUpdatesDisposable?.Dispose();
                }
            });

        _checkForSchemaVersionDisposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(10))
            .Select(time => Unit.Default)
            .Subscribe(async _ =>
            {
                try
                {
                    await CheckForSchemaVersionAsync();
                }
                catch (Exception ex)
                {
                    var message = "Failed to check for schema version. Please check your internet connection or try again later.";
                    this.Log()
                        .Error(ex, message);
                    _notificationService.Notify("Needlework.Net", message, InfoBarSeverity.Error);
                    _checkForSchemaVersionDisposable?.Dispose();
                }
            });

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    [ObservableProperty]
    private ObservableCollection<NotificationViewModel> _notifications = [];

    [ObservableProperty]
    private NavigationViewItem _selectedNavigationViewItem;

    [ObservableProperty]
    private PageBase _currentPage;

    public List<NavigationViewItem> NavigationViewItems { get; private set; } = [];

    public bool IsSchemaVersionChecked { get; private set; }

    public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";

    public string Title => $"Needlework.Net {Version}";

    private NavigationViewItem ToNavigationViewItem(PageBase page) => new()
    {
        Content = page.DisplayName,
        Tag = page,
        IconSource = new ImageIconSource
        {
            Source = new Projektanker.Icons.Avalonia.IconImage()
            {
                Value = page.Icon,
                Brush = new SolidColorBrush(Application.Current!.ActualThemeVariant.Key switch
                {
                    "Light" => Colors.Black,
                    "Dark" => Colors.White,
                    _ => Colors.Gray
                })
            }
        }
    };

    partial void OnSelectedNavigationViewItemChanged(NavigationViewItem value)
    {
        if (value.Tag is PageBase page)
        {
            CurrentPage = page;
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        var release = await _githubClient
            .Request("/repos/BlossomiShymae/Needlework.Net/releases/latest")
            .WithHeader("User-Agent", $"Needlework.Net/{Version}")
            .GetJsonAsync<GithubRelease>();

        if (release.IsLatest(Version))
        {
            this.Log()
                .Information("New version available: {TagName}", release.TagName);
            _notificationService.Notify("Needlework.Net", $"New version available: {release.TagName}", InfoBarSeverity.Informational, null, "https://github.com/BlossomiShymae/Needlework.Net/releases/latest");
            _checkForUpdatesDisposable?.Dispose();
        }
    }


    private async Task CheckForSchemaVersionAsync()
    {
        if (!ProcessFinder.IsPortOpen()) return;

        var lcuSchemaDocument = await _documentService.GetLcuSchemaDocumentAsync();
        var client = Connector.GetLcuHttpClientInstance();
        var currentSemVer = lcuSchemaDocument.Info.Version.Split('.');
        var systemBuild = await client.GetFromJsonAsync<SystemBuild>("/system/v1/builds") ?? throw new NullReferenceException();
        var latestSemVer = systemBuild.Version.Split('.');

        if (!IsSchemaVersionChecked)
        {
            this.Log()
                .Information("LCU Schema (current): {Version}", lcuSchemaDocument.Info.Version);
            this.Log()
                .Information("LCU Schema (latest): {Version}", systemBuild.Version);
            IsSchemaVersionChecked = true;
        }

        bool isVersionMatching = currentSemVer[0] == latestSemVer[0] && currentSemVer[1] == latestSemVer[1]; // Compare major and minor versions
        if (!isVersionMatching)
        {
            this.Log()
                .Warning("LCU Schema version mismatch: Current {CurrentVersion}, Latest {LatestVersion}", lcuSchemaDocument.Info.Version, systemBuild.Version);
            _notificationService.Notify("Needlework.Net", $"LCU Schema is possibly outdated compared to latest system build. Consider submitting a pull request on dysolix/hasagi-types.\nCurrent: {string.Join(".", currentSemVer)}\nLatest: {string.Join(".", latestSemVer)}", InfoBarSeverity.Warning, null, "https://github.com/dysolix/hasagi-types#updating-the-types");
            _checkForSchemaVersionDisposable?.Dispose();
        }
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        var process = new Process() { StartInfo = new ProcessStartInfo(url) { UseShellExecute = true } };
        process.Start();
    }

    public void Receive(OopsiesDialogRequestedMessage message)
    {
        Avalonia.Threading.Dispatcher.UIThread.Invoke(async () => await _dialogService.ShowAsync<OopsiesDialog>(message.Value));
    }
}
