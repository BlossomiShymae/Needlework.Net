using Avalonia;
using Avalonia.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.Extensions;
using Needlework.Net.Models;
using Needlework.Net.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Home;

public partial class HomeViewModel : PageBase, IEnableLogger
{
    private readonly HextechDocsService _hextechDocsService;

    private readonly IDisposable _carouselNextDisposable;

    public HomeViewModel(HextechDocsService hextechDocsService) : base("Home", "fa-solid fa-house")
    {
        _hextechDocsService = hextechDocsService;

        _carouselNextDisposable = Observable.Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
            .Select(time => Unit.Default)
            .Subscribe(_ =>
            {
                if (SelectedHextechDocsPost is HextechDocsPostViewModel vm)
                {
                    var index = HextechDocsPosts.IndexOf(vm);
                    if (index == HextechDocsPosts.Count - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index += 1;
                    }
                    SelectedHextechDocsPost = HextechDocsPosts.ElementAt(index);
                }
            });
    }

    public List<LibraryViewModel> Libraries { get; } = JsonSerializer.Deserialize<List<Library>>(AssetLoader.Open(new Uri($"avares://NeedleworkDotNet/Assets/libraries.json")))
        !.Where(library => library.Tags.Contains("lcu") || library.Tags.Contains("ingame"))
        .Select(library => new LibraryViewModel(library))
        .ToList();

    [ObservableProperty]
    private Vector _librariesOffset = new();

    [ObservableProperty]
    private List<HextechDocsPostViewModel> _hextechDocsPosts = [];

    [ObservableProperty]
    private HextechDocsPostViewModel? _selectedHextechDocsPost;

    public override async Task InitializeAsync()
    {
        try
        {
            var posts = await _hextechDocsService.GetPostsAsync();
            var hextechDocsPosts = posts.Select(post => new HextechDocsPostViewModel(post)).ToList();
            Dispatcher.UIThread.Invoke(() =>
            {
                HextechDocsPosts = hextechDocsPosts;
                SelectedHextechDocsPost = HextechDocsPosts.First();
            });
        }
        catch (Exception ex)
        {
            this.Log()
                .Error(ex, "Failed to get posts from HextechDocs.");
        }
    }
}
