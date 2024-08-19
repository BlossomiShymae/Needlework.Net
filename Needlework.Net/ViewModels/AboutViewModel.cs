using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Net.Http;

namespace Needlework.Net.ViewModels
{
    public partial class AboutViewModel : PageBase
    {
        public HttpClient HttpClient { get; }

        public AboutViewModel(HttpClient httpClient) : base("About", "info-circle")
        {
            HttpClient = httpClient;
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
}
