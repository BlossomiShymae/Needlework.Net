using BlossomiShymae.Briar.Utils;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Needlework.Net.ViewModels.Pages.Endpoints;

public partial class ResponseViewModel : ReactiveObject
{
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

    [Reactive]
    private string? _path;

    [Reactive]
    private string? _status;

    [Reactive]
    private string? _authentication;

    [Reactive]
    private string? _username;

    [Reactive]
    private string? _password;

    [Reactive]
    private string? _authorization;

    private static ProcessInfo? GetProcessInfo()
    {
        if (ProcessFinder.IsActive()) return ProcessFinder.GetProcessInfo();
        return null;
    }
}
