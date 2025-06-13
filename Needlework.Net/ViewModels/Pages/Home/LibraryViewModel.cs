using Needlework.Net.Models;
using ReactiveUI;

namespace Needlework.Net.ViewModels.Pages.Home
{
    public class LibraryViewModel : ReactiveObject
    {
        public LibraryViewModel(Library library)
        {
            Library = library;
        }

        public Library Library { get; }
    }
}
