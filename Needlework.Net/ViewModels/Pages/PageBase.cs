using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;


public abstract partial class PageBase(string displayName, string icon) : ObservableValidator
{
    public string DisplayName { get; } = displayName;

    public string Icon { get; } = icon;

    public abstract Task InitializeAsync();
}