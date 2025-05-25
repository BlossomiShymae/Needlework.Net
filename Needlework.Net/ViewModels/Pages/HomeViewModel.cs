using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;

public partial class HomeViewModel : PageBase
{
    public List<Library> Libraries { get; } = JsonSerializer.Deserialize<List<Library>>(AssetLoader.Open(new Uri($"avares://NeedleworkDotNet/Assets/libraries.json")))!;

    public HomeViewModel() : base("Home", "home", int.MinValue) { }

    public override Task InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(url) { UseShellExecute = true }
        };
        process.Start();
    }

}
