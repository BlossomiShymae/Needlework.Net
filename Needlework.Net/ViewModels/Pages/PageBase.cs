using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace Needlework.Net.ViewModels.Pages;


public abstract partial class PageBase(string displayName, string icon, int index = 0) : ObservableValidator
{
    [ObservableProperty] private string _displayName = displayName;
    [ObservableProperty] private string _icon = icon;
    [ObservableProperty] private int _index = index;
    [ObservableProperty] private bool _isInitialized;

    public abstract Task InitializeAsync();
}