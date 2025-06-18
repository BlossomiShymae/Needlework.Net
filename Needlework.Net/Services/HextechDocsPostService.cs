using AngleSharp;
using FastCache;
using Needlework.Net.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Needlework.Net.Services
{
    public class HextechDocsPostService
    {
        private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());

        public async Task<List<HextechDocsPost>> GetPostsAsync()
        {
            if (Cached<List<HextechDocsPost>>.TryGet(nameof(GetPostsAsync), out var cached))
            {
                return cached;
            }

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

            return cached.Save(posts, TimeSpan.FromMinutes(60));
        }
    }
}
