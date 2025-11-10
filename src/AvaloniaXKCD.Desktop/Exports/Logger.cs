using System;
using System.Diagnostics;
using System.IO;
using AvaloniaXKCD.Exports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace AvaloniaXKCD.Desktop;

public class DesktopLogger : Exports.ILogger
{
    private readonly Microsoft.Extensions.Logging.ILoggerFactory _innerFactory;

    public DesktopLogger()
    {
        _innerFactory = LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = false;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            })
            .SetMinimumLevel(MapLevel(App.Config.LogLevel))
        );
    }

    private Microsoft.Extensions.Logging.LogLevel MapLevel(Exports.LogLevel level) =>
        level switch
        {
            Exports.LogLevel.Trace => Microsoft.Extensions.Logging.LogLevel.Trace,
            Exports.LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
            Exports.LogLevel.Information => Microsoft.Extensions.Logging.LogLevel.Information,
            Exports.LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
            Exports.LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
            Exports.LogLevel.Critical => Microsoft.Extensions.Logging.LogLevel.Critical,
            _ => Microsoft.Extensions.Logging.LogLevel.None
        };

    public bool ShouldLog(Exports.LogLevel level) => level.ShouldLog(App.Config.LogLevel);

    public void Log(Exports.LogLevel level, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
    {
        var category = Path.GetFileNameWithoutExtension(file);
        var logger = _innerFactory.CreateLogger(category ?? "Logger");
        logger.Log(MapLevel(level), message);
    }

    public void Log(Exports.LogLevel level, Exception ex,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => Log(level, ex.Message, file);
}