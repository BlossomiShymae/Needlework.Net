using Avalonia.Platform;
using Needlework.Net.Models;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Needlework.Net.ViewModels.Pages.Home;

public partial class HomeViewModel : PageBase
{
    public HomeViewModel(IScreen? screen = null) : base("Home", "home", int.MinValue)
    {
        Libraries = JsonSerializer.Deserialize<List<Library>>(AssetLoader.Open(new Uri($"avares://NeedleworkDotNet/Assets/libraries.json")))!
            .Select(library => new LibraryViewModel(library))
            .ToList();
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
    }
    public List<LibraryViewModel> Libraries { get; }

    public override string? UrlPathSegment => "home";

    public override IScreen HostScreen { get; }
}
