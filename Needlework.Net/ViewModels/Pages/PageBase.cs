using ReactiveUI;

namespace Needlework.Net.ViewModels.Pages;


public abstract partial class PageBase(string displayName, string icon, int index = 0) : ReactiveObject, IRoutableViewModel
{
    public string DisplayName { get; } = displayName;

    public string Icon { get; } = icon;

    public int Index { get; } = index;

    public abstract string? UrlPathSegment { get; }

    public abstract IScreen HostScreen { get; }
}