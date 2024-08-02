using BlossomiShymae.GrrrLCU;

namespace Needlework.Net.Core;

public static class LcuConnector
{
    public static Func<ProcessInfo> GetProcessInfo { get; } = Connector.GetProcessInfo;
    public static Func<int, string, Uri> GetLeagueClientUri { get; } = Connector.GetLeagueClientUri;
    public static Func<HttpMethod, string, CancellationToken, Task<HttpResponseMessage>> SendAsync { get; } = Connector.SendAsync;
}