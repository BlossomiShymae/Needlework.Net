using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.Models;

namespace Needlework.Net.ViewModels.Pages.Home
{
    public partial class LibraryViewModel : ObservableObject
    {
        public LibraryViewModel(Library library)
        {
            Library = library;
        }

        public Library Library { get; }
    }
}
