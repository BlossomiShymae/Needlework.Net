using Akavache;
using Avalonia.Threading;
using BlossomiShymae.Briar;
using BlossomiShymae.Briar.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Needlework.Net.Constants;
using Needlework.Net.DataModels;
using Needlework.Net.Extensions;
using Needlework.Net.Models;
using Needlework.Net.Services;
using System;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Settings
{
    public partial class SettingsViewModel : PageBase, IEnableLogger
    {
        private readonly IBlobCache _blobCache;

        private readonly IDisposable _checkForUpdatesDisposable;

        private readonly IDisposable _checkForSchemaVersionDisposable;

        private readonly GithubService _githubService;

        private readonly DocumentService _documentService;

        private readonly NotificationService _notificationService;

        private readonly TaskCompletionSource<bool> _initializeTaskCompletionSource = new();

        public SettingsViewModel(IBlobCache blobCache, GithubService githubService, DocumentService documentService, NotificationService notificationService) : base("Settings", "fa-solid fa-gear")
        {
            _blobCache = blobCache;
            _githubService = githubService;
            _documentService = documentService;
            _notificationService = notificationService;

            _checkForUpdatesDisposable = Observable.Timer(TimeSpan.Zero, Intervals.CheckForUpdates)
                .Select(time => Unit.Default)
                .Subscribe(async _ =>
                {
                    try
                    {
                        await _initializeTaskCompletionSource.Task;
                        if (AppSettings!.IsCheckForUpdates)
                        {
                            await CheckForUpdatesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = "Failed to check for updates. Please check your internet connection or try again later.";
                        this.Log()
                            .Error(ex, message);
                        _notificationService.Notify(AppInfo.Name, message, InfoBarSeverity.Error);
                        _checkForUpdatesDisposable?.Dispose();
                    }
                });

            _checkForSchemaVersionDisposable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMinutes(5))
                .Select(time => Unit.Default)
                .Subscribe(async _ =>
                {
                    try
                    {
                        await _initializeTaskCompletionSource.Task;
                        if (AppSettings!.IsCheckForSchema)
                        {
                            await CheckForSchemaVersionAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = "Failed to check for schema version. Please check your internet connection or try again later.";
                        this.Log()
                            .Error(ex, message);
                        _notificationService.Notify(AppInfo.Name, message, InfoBarSeverity.Error);
                        _checkForSchemaVersionDisposable?.Dispose();
                    }
                });
        }

        [ObservableProperty]
        private bool _isBusy = true;

        [ObservableProperty]
        private AppSettings? _appSettings;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UpdateCheckTitle), nameof(UpdateCheckIconValue), nameof(UpdateCheckLastChecked))]
        private Guid _upToDateGuid = Guid.Empty;

        public bool IsUpToDate { get; private set; }

        public bool IsSchemaVersionChecked { get; private set; }

        public string UpdateCheckTitle => IsUpToDate switch
        {
            true => "You're up to date",
            false => "You're out of date"
        };

        public string UpdateCheckIconValue => IsUpToDate switch
        {
            true => "fa-heart-circle-check",
            false => "fa-heart-circle-exclamation"
        };

        public string UpdateCheckLastChecked => $"Last checked: {DateTime.Now:dddd}, {DateTime.Now:T}";

        partial void OnAppSettingsChanged(AppSettings? value)
        {
            if (AppSettings is AppSettings appSettings)
            {
                _blobCache.InsertObject(BlobCacheKeys.AppSettings, appSettings);
            }
        }

        public override async Task InitializeAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    AppSettings = await _blobCache.GetObject<AppSettings>(BlobCacheKeys.AppSettings);
                }
                catch (Exception ex)
                {
                    this.Log()
                        .Warning(ex, "Failed to get application settings.");
                    AppSettings = new();
                }
                finally
                {
                    AppSettings!.PropertyChanged += (s, e) => OnAppSettingsChanged((AppSettings?)s);
                    IsBusy = false;
                    _initializeTaskCompletionSource.SetResult(true);
                }
            });
        }

        [RelayCommand]
        private async Task CheckForUpdatesAsync()
        {
            var release = await _githubService.GetLatestReleaseAsync();
            if (release.IsLatest(AppInfo.Version))
            {
                this.Log()
                    .Information("New version available: {TagName}", release.TagName);
                _notificationService.Notify(AppInfo.Name, $"New version available: {release.TagName}", InfoBarSeverity.Informational, null, "https://github.com/BlossomiShymae/Needlework.Net/releases/latest");
                _checkForUpdatesDisposable?.Dispose();
                IsUpToDate = false;
            }
            else
            {
                IsUpToDate = true;
            }
            UpToDateGuid = Guid.NewGuid();
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
                    .Warning("LCU Schema outdated: Current {CurrentVersion}, Latest {LatestVersion}", lcuSchemaDocument.Info.Version, systemBuild.Version);
                _notificationService.Notify(AppInfo.Name, $"LCU Schema is outdated compared to latest system build. Consider submitting a pull request on dysolix/hasagi-types.\nCurrent: {string.Join(".", currentSemVer)}\nLatest: {string.Join(".", latestSemVer)}", InfoBarSeverity.Warning, null, "https://github.com/dysolix/hasagi-types#updating-the-types");
                _checkForSchemaVersionDisposable?.Dispose();
            }
        }
    }
}
