using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages
{
    public class PageFactory
    {
        private readonly IEnumerable<PageBase> _pages;

        public PageFactory(IEnumerable<PageBase> pages)
        {
            _pages = pages;
        }

        public PageBase GetPage<T>() where T : PageBase
        {
            var page = _pages.Where(page => typeof(T) == page.GetType())
                .FirstOrDefault() ?? throw new NotSupportedException(typeof(T).FullName);
            Task.Run(page.InitializeAsync);
            return page;
        }
    }
}
