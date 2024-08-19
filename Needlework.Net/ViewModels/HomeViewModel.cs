using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Needlework.Net.ViewModels
{
    public partial class HomeViewModel : PageBase
    {
        public HomeViewModel() : base("Home", "home", int.MinValue) { }

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
