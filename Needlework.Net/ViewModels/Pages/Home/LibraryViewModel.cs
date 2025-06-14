using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Needlework.Net.Models;
using System.Diagnostics;

namespace Needlework.Net.ViewModels.Pages.Home
{
    public partial class LibraryViewModel : ObservableObject
    {
        public LibraryViewModel(Library library)
        {
            Library = library;
        }

        public Library Library { get; }

        [RelayCommand]
        private void OpenUrl()
        {
            var process = new Process() { StartInfo = new ProcessStartInfo(Library.Link) { UseShellExecute = true } };
            process.Start();
        }
    }
}
