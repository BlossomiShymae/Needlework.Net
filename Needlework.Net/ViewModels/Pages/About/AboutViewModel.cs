using ReactiveUI;
using Splat;

namespace Needlework.Net.ViewModels.Pages.About;

public partial class AboutViewModel : PageBase
{
    public AboutViewModel(IScreen? screen = null) : base("About", "info-circle")
    {
        HostScreen = screen ?? Locator.Current.GetService<IScreen>()!;
    }

    public override string? UrlPathSegment => "about";

    public override IScreen HostScreen { get; }
}
