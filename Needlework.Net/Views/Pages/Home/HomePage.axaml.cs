using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.Home;
using ReactiveUI;

namespace Needlework.Net.Views.Pages.Home;

public partial class HomePage : ReactiveUserControl<HomeViewModel>
{
    public HomePage()
    {
        this.WhenActivated(disposables => { });

        InitializeComponent();
    }
}
