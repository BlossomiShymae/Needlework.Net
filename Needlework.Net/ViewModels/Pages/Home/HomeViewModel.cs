using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.Home;

public partial class HomeViewModel : PageBase
{
    public HomeViewModel() : base("Home", "fa-solid fa-house", int.MinValue) { }

    public List<LibraryViewModel> Libraries { get; } = JsonSerializer.Deserialize<List<Library>>(AssetLoader.Open(new Uri($"avares://NeedleworkDotNet/Assets/libraries.json")))
        !.Where(library => library.Tags.Contains("lcu") || library.Tags.Contains("ingame"))
        .Select(library => new LibraryViewModel(library))
        .ToList();

    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenUrl(string? value)
    {
        if (value == null) return;
        var process = new Process() { StartInfo = new ProcessStartInfo(value) { UseShellExecute = true } };
        process.Start();
    }

}
