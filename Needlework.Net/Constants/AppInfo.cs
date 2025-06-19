using System.Reflection;

namespace Needlework.Net.Constants
{
    public static class AppInfo
    {
        public static readonly string Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";
    }
}
