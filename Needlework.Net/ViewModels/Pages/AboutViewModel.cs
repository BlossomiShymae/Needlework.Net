using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;

public partial class AboutViewModel : PageBase
{
    public HttpClient HttpClient { get; }

    public AboutViewModel(HttpClient httpClient) : base("About", "info-circle")
    {
        HttpClient = httpClient;
    }

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
