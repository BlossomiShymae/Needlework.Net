using Avalonia.ReactiveUI;
using Needlework.Net.ViewModels.Pages.About;
using ReactiveUI;

namespace Needlework.Net.Views.Pages.About;

public partial class AboutPage : ReactiveUserControl<AboutViewModel>
{
    public AboutPage()
    {
        this.WhenActivated(disposables => { });

        InitializeComponent();
    }
}