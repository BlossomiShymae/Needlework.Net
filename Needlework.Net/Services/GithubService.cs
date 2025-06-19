using Akavache;
using Flurl.Http;
using Flurl.Http.Configuration;
using Needlework.Net.Constants;
using Needlework.Net.Extensions;
using Needlework.Net.Models;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Needlework.Net.Services
{
    public class GithubService : IEnableLogger
    {
        private readonly IFlurlClient _githubClient;

        private readonly IFlurlClient _githubUserContentClient;

        private readonly IBlobCache _blobCache;

        public GithubService(IBlobCache blobCache, IFlurlClientCache clients)
        {
            _githubClient = clients.Get("GithubClient");
            _githubUserContentClient = clients.Get("GithubUserContentClient");
            _blobCache = blobCache;
        }

        public async Task<GithubRelease> GetLatestReleaseAsync()
        {
            return await _blobCache.GetOrFetchObject(BlobCacheKeys.GithubLatestRelease, async () =>
            {
                this.Log()
                    .Debug("Downloading latest release info from GitHub...");
                var release = await _githubClient
                    .Request("/repos/BlossomiShymae/Needlework.Net/releases/latest")
                    .WithHeader("User-Agent", $"Needlework.Net/{AppInfo.Version}")
                    .GetJsonAsync<GithubRelease>();
                return release;
            }, DateTimeOffset.Now + Intervals.CheckForUpdates);
        }
    }
}
