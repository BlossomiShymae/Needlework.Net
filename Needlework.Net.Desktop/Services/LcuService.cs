using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.Core;
using System.Net.Http;
using System.Threading.Tasks;

namespace Needlework.Net.Desktop.Services
{
    public partial class LcuService : ObservableObject
    {
        public HttpClient HttpClient { get; }

        public LcuSchemaHandler LcuSchemaHandler { get; }

        [ObservableProperty] private string _statusText = "Offline";
        [ObservableProperty] private IBrush _statusColor = new SolidColorBrush(Colors.Red.ToUInt32());
        [ObservableProperty] private string _statusAddress = "N/A";

        public LcuService(HttpClient httpClient)
        {
            HttpClient = httpClient;

            Task.Run(ProcessBackground);
        }

        private void ProcessBackground()
        {

        }
    }
}
