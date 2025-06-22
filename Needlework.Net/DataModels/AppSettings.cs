using CommunityToolkit.Mvvm.ComponentModel;

namespace Needlework.Net.DataModels
{
    public partial class AppSettings : ObservableObject
    {
        [ObservableProperty]
        private bool _isCheckForUpdates = true;

        [ObservableProperty]
        private bool _isCheckForSchema = true;
    }
}
