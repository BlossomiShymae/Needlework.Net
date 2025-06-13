using BlossomiShymae.Briar;
using BlossomiShymae.Briar.Utils;
using FluentAvalonia.UI.Controls;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;
using Needlework.Net.Models;
using Needlework.Net.Services;
using Needlework.Net.ViewModels.Pages;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.MainWindow;

public partial class MainWindowViewModel
    : ReactiveObject, IScreen, IEnableLogger
{
    private readonly IEnumerable<PageBase> _pages;

    private readonly IFlurlClient _githubClient;

    private readonly NotificationService _notificationService;

    private readonly DataSource _dataSource;

    private readonly IDisposable _checkForUpdatesDisposable;

    private readonly IDisposable _checkForSchemaVersionDisposable;

    public MainWindowViewModel(IEnumerable<PageBase>? pages = null, IFlurlClientCache? clients = null, NotificationService? notificationService = null, DataSource? dataSource = null)
    {
        _pages = pages ?? Locator.Current.GetServices<PageBase>();
        _githubClient = clients?.Get("GithubClient") ?? Locator.Current.GetService<IFlurlClientCache>()!.Get("GithubClient");
        _notificationService = notificationService ?? Locator.Current.GetService<NotificationService>()!;
        _dataSource = dataSource ?? Locator.Current.GetService<DataSource>()!;

        PageItems = _pages
            .OrderBy(p => p.Index)
            .ThenBy(p => p.DisplayName)
            .Select(ToNavigationViewItem)
            .ToList();

        SelectedPageItem = PageItems.First();

        this.WhenAnyValue(x => x.SelectedPageItem)
            .Subscribe(x => Router.Navigate.Execute((IRoutableViewModel)x.Tag!));

        _notificationService.Notifications.Subscribe(async notification =>
        {
            var vm = new NotificationViewModel(notification);
            Notifications.Add(vm);
            await Task.Delay(notification.Duration ?? TimeSpan.FromSeconds(10));
            Notifications.Remove(vm);
        });

        CheckForUpdatesCommand.ThrownExceptions.Subscribe(ex =>
        {
            var message = "Failed to check for updates. Please check your internet connection or try again later.";
            this.Log()
                .Error(ex, message);
            _notificationService.Notify("Needlework.Net", message, InfoBarSeverity.Error);
            _checkForUpdatesDisposable?.Dispose();
        });

        _checkForUpdatesDisposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(10))
            .Select(time => Unit.Default)
            .InvokeCommand(this, x => x.CheckForUpdatesCommand);

        CheckForSchemaVersionCommand.ThrownExceptions.Subscribe(ex =>
        {
            var message = "Failed to check for schema version. Please check your internet connection or try again later.";
            this.Log()
                .Error(ex, message);
            _notificationService.Notify("Needlework.Net", message, InfoBarSeverity.Error);
            _checkForSchemaVersionDisposable?.Dispose();
        });

        _checkForSchemaVersionDisposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(10))
            .Select(time => Unit.Default)
            .InvokeCommand(this, x => x.CheckForSchemaVersionCommand);
    }

    [Reactive]
    private RoutingState _router = new();

    [Reactive]
    private ObservableCollection<NotificationViewModel> _notifications = [];

    [Reactive]
    private NavigationViewItem _selectedPageItem;

    public List<NavigationViewItem> PageItems = [];

    public bool IsSchemaVersionChecked { get; private set; } = false;

    public string Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";

    private NavigationViewItem ToNavigationViewItem(PageBase page) => new()
    {
        Content = page.DisplayName,
        Tag = page,
        IconSource = new BitmapIconSource() { UriSource = new Uri($"avares://NeedleworkDotNet/Assets/Icons/{page.Icon}.png") }
    };

    [ReactiveCommand]
    private async Task CheckForUpdatesAsync()
    {
        var release = await _githubClient
            .Request("/repos/BlossomiShymae/Needlework.Net/releases/latest")
            .WithHeader("User-Agent", $"Needlework.Net/{Version}")
            .GetJsonAsync<GithubRelease>();

        if (release.IsLatest(Version))
        {
            this.Log()
                .Info("New version available: {TagName}", release.TagName);
            _notificationService.Notify("Needlework.Net", $"New version available: {release.TagName}", InfoBarSeverity.Informational, null, "https://github.com/BlossomiShymae/Needlework.Net/releases/latest");
            _checkForUpdatesDisposable?.Dispose();
        }
    }


    [ReactiveCommand]
    private async Task CheckForSchemaVersionAsync()
    {
        if (!ProcessFinder.IsPortOpen()) return;

        var lcuSchemaDocument = await _dataSource.GetLcuSchemaDocumentAsync();
        var client = Connector.GetLcuHttpClientInstance();
        var currentSemVer = lcuSchemaDocument.Info.Version.Split('.');
        var systemBuild = await client.GetFromJsonAsync<SystemBuild>("/system/v1/builds") ?? throw new NullReferenceException();
        var latestSemVer = systemBuild.Version.Split('.');

        if (!IsSchemaVersionChecked)
        {
            this.Log()
                .Info("LCU Schema (current): {Version}", lcuSchemaDocument.Info.Version);
            this.Log()
                .Info("LCU Schema (latest): {Version}", systemBuild.Version);
            IsSchemaVersionChecked = true;
        }

        bool isVersionMatching = currentSemVer[0] == latestSemVer[0] && currentSemVer[1] == latestSemVer[1]; // Compare major and minor versions
        if (!isVersionMatching)
        {
            this.Log()
                .Warn("LCU Schema version mismatch: Current {CurrentVersion}, Latest {LatestVersion}", lcuSchemaDocument.Info.Version, systemBuild.Version);
            _notificationService.Notify("Needlework.Net", $"LCU Schema is possibly outdated compared to latest system build. Consider submitting a pull request on dysolix/hasagi-types.\nCurrent: {string.Join(".", currentSemVer)}\nLatest: {string.Join(".", latestSemVer)}", InfoBarSeverity.Warning, null, "https://github.com/dysolix/hasagi-types#updating-the-types");
            _checkForSchemaVersionDisposable?.Dispose();
        }
    }
}
