using BlossomiShymae.Briar.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class ResponseViewModel : ObservableObject
{
    [ObservableProperty] private string? _path;
    [ObservableProperty] private string? _status;
    [ObservableProperty] private string? _authentication;
    [ObservableProperty] private string? _username;
    [ObservableProperty] private string? _password;
    [ObservableProperty] private string? _authorization;

    public ResponseViewModel(string path)
    {
        Path = path;
        var processInfo = GetProcessInfo();
        if (processInfo != null)
        {
            var riotAuthentication = new RiotAuthentication(processInfo.RemotingAuthToken);
            Path = $"https://127.0.0.1:{processInfo.AppPort}{path}";
            Username = riotAuthentication.Username;
            Password = riotAuthentication.Password;
            Authorization = $"Basic {riotAuthentication.RawValue}";
        }
    }

    private static ProcessInfo? GetProcessInfo()
    {
        if (ProcessFinder.IsActive()) return ProcessFinder.GetProcessInfo();
        return null;
    }
}
