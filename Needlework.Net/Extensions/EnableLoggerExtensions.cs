using Serilog;

namespace Needlework.Net.Extensions
{
    public static class EnableLoggerExtensions
    {
        private static readonly ILogger _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}", path: "Logs/debug-.log", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();

        public static ILogger Log(this IEnableLogger? context) => _logger.ForContext(context?.GetType() ?? typeof(Program));
    }

    public interface IEnableLogger;
}
