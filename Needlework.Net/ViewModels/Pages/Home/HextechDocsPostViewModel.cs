using CommunityToolkit.Mvvm.ComponentModel;
using Needlework.Net.DataModels;

namespace Needlework.Net.ViewModels.Pages.Home
{
    public partial class HextechDocsPostViewModel : ObservableObject
    {
        public HextechDocsPostViewModel(HextechDocsPost hextechDocsPost)
        {
            HextechDocsPost = hextechDocsPost;
        }

        public HextechDocsPost HextechDocsPost { get; }
    }
}
