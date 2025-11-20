namespace AvaloniaXKCD.Tests;

public class DesktopLogger : BaseLogger
{
    TUnit.Core.Logging.LogLevel ConvertLogLevel(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.None => TUnit.Core.Logging.LogLevel.None,
            LogLevel.Trace => TUnit.Core.Logging.LogLevel.Trace,
            LogLevel.Debug => TUnit.Core.Logging.LogLevel.Debug,
            LogLevel.Information => TUnit.Core.Logging.LogLevel.Information,
            LogLevel.Warning => TUnit.Core.Logging.LogLevel.Warning,
            LogLevel.Error => TUnit.Core.Logging.LogLevel.Error,
            LogLevel.Critical => TUnit.Core.Logging.LogLevel.Critical,
            _ => throw new NotImplementedException(),
        };

    internal static readonly Func<string, Exception?, string> Formatter = (state, exception) =>
        exception is not null
            ? $"{state}{Environment.NewLine}------Exception detail------{Environment.NewLine}{exception}"
            : state;

    public override bool ShouldLog(LogLevel level)
    {
        if (TestContext.Current == null)
            return false;
        var tunitLevel = ConvertLogLevel(level);
        return TestContext.Current.GetDefaultLogger().IsEnabled(tunitLevel);
    }

    public override void Log(
        LogLevel level,
        string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null
    )
    {
        if (TestContext.Current == null)
            return;
        var tunitLevel = ConvertLogLevel(level);
        var logger = TestContext.Current.GetDefaultLogger();

        logger.Log(tunitLevel, message, null, Formatter);
    }

    public override void Log(
        LogLevel level,
        Exception err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null
    ) => Log(level, err.ToString());
}
