namespace AvaloniaXKCD.Exports;

public interface ILogger : IExport
{
    bool ShouldLog(LogLevel level);

    void Log(LogLevel level, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null);
    public void Log(LogLevel level, Exception err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null);
}

public abstract class BaseLogger : ILogger
{
    public bool ShouldLogTrace { get => ShouldLog(LogLevel.Trace); }
    public bool ShouldLogDebug { get => ShouldLog(LogLevel.Debug); }
    public bool ShouldLogInformation { get => ShouldLog(LogLevel.Information); }
    public bool ShouldLogWarning { get => ShouldLog(LogLevel.Warning); }
    public bool ShouldLogError { get => ShouldLog(LogLevel.Error); }
    public bool ShouldLogCritical { get => ShouldLog(LogLevel.Critical); }
    public abstract bool ShouldLog(LogLevel level);


    public abstract void Log(LogLevel level, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null);
    public abstract void Log(LogLevel level, Exception err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null);
}

public static class LoggerExtensions
{
    public static void LogTrace(this ILogger logger, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Trace, message, file);
    public static void LogDebug(this ILogger logger, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Debug, message, file);
    public static void LogInformation(this ILogger logger, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Information, message, file);
    public static void LogWarning(this ILogger logger, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Warning, message, file);
    public static void LogError(this ILogger logger, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Error, message, file);
    public static void LogError(this ILogger logger, Exception err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Error, err, file);
    public static void LogCritical(this ILogger logger, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Critical, message, file);
    public static void LogCritical(this ILogger logger, Exception err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.Log(LogLevel.Error, err, file);

    public static bool IfShouldLog(
        this ILogger logger,
        LogLevel level,
        Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
    {
        if (logger.ShouldLog(level))
        {
            logger.Log(level, message(), file);
            return true;
        }
        return false;
    }
    public static bool IfShouldLog(
        this ILogger logger,
        LogLevel level,
        Func<Exception> err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
    {
        if (logger.ShouldLog(level))
        {
            logger.Log(level, err(), file);
            return true;
        }
        return false;
    }

    public static bool IfShouldLogTrace(this ILogger logger, Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Trace, message, file);
    public static bool IfShouldLogDebug(this ILogger logger, Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Debug, message, file);
    public static bool IfShouldLogInformation(this ILogger logger, Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Information, message, file);
    public static bool IfShouldLogWarning(this ILogger logger, Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Warning, message, file);
    public static bool IfShouldLogError(this ILogger logger, Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Error, message, file);
    public static bool IfShouldLogError(this ILogger logger, Func<Exception> err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Error, err, file);
    public static bool IfShouldLogCritical(this ILogger logger, Func<string> message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Critical, message, file);
    public static bool IfShouldLogCritical(this ILogger logger, Func<Exception> err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => logger.IfShouldLog(LogLevel.Error, err, file);

    public static bool ShouldLog(this LogLevel currentLevel, LogLevel desiredLevel)
        => currentLevel >= desiredLevel;
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    None
}