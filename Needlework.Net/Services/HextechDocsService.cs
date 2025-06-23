using Akavache;
using AngleSharp;
using Needlework.Net.DataModels;
using Needlework.Net.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Needlework.Net.Services
{
    public class HextechDocsService : IEnableLogger
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

        private readonly IBlobCache _blobCache;

        public HextechDocsService(IBlobCache blobCache)
        {
            _blobCache = blobCache;
        }

        public async Task<List<HextechDocsPost>> GetPostsAsync()
        {
            return await _blobCache.GetOrFetchObject("HextechDocsPosts", async () =>
            {
                this.Log()
                    .Debug("Downloading HextechDocs posts...");
                var document = await _context.OpenAsync("https://hextechdocs.dev/tag/lcu/");
                var elements = document.QuerySelectorAll("article.post-card");
                var posts = new List<HextechDocsPost>();
                foreach (var element in elements)
                {
                    var path = element.QuerySelector("a.post-card-content-link")!.GetAttribute("href")!;
                    var title = element.QuerySelector(".post-card-title")!.TextContent;
                    var excerpt = element.QuerySelector(".post-card-excerpt > p")!.TextContent;
                    var post = new HextechDocsPost()
                    {
                        Path = path,
                        Title = title,
                        Excerpt = excerpt,
                    };
                    posts.Add(post);
                }
                return posts;
            }, DateTimeOffset.Now + TimeSpan.FromHours(12));
        }
    }
}
