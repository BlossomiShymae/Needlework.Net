using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Needlework.Net
{
    public static class Logger
    {
        public static void Setup(ILoggingBuilder builder)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("Logs/debug-.log", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
            logger.Debug("NeedleworkDotNet version: {Version}", Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0");
            logger.Debug("OS description: {Description}", System.Runtime.InteropServices.RuntimeInformation.OSDescription);
            builder.AddSerilog(logger);
        }

        public static void LogFatal(UnhandledExceptionEventArgs e)
        {
            File.WriteAllText($"Logs/fatal-{DateTime.Now:HHmmssfff}.log", e.ExceptionObject.ToString());
        }
    }
}
