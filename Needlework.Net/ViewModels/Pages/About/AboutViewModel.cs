using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages.About;

public partial class AboutViewModel : PageBase
{
    public AboutViewModel() : base("About", "info-circle")
    {
    }

    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        var process = new Process() { StartInfo = new ProcessStartInfo(url) { UseShellExecute = true } };
        process.Start();
    }
}
