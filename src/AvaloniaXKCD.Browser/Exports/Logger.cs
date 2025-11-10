using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Browser;

[SupportedOSPlatform("browser")]
public partial class BrowserLogger : ILogger
{
    [JSImport("globalThis.console.log")]
    internal static partial Task Log([JSMarshalAs<JSType.String>] string message);
    [JSImport("globalThis.console.info")]
    internal static partial Task Info([JSMarshalAs<JSType.String>] string message);
    [JSImport("globalThis.console.warn")]
    internal static partial Task Warn([JSMarshalAs<JSType.String>] string message);
    [JSImport("globalThis.console.debug")]
    internal static partial Task Debug([JSMarshalAs<JSType.String>] string message);
    [JSImport("globalThis.console.error")]
    internal static partial Task Error([JSMarshalAs<JSType.String>] string message);
    [JSImport("globalThis.console.trace")]
    internal static partial Task Trace([JSMarshalAs<JSType.String>] string message);


    public bool ShouldLog(LogLevel level) => level.ShouldLog(App.Config.LogLevel);
    public void Log(LogLevel level, string message,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
    {
        var messageTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var category = Path.GetFileNameWithoutExtension(file);
        switch (level)
        {
            case LogLevel.None:
                break;
            case LogLevel.Trace:
                Trace($"{messageTime} {level}: {category} - {message}");
                break;
            case LogLevel.Debug:
                Debug($"{messageTime} {level}: {category} - {message}");
                break;
            case LogLevel.Information:
                Info($"{messageTime} {level}: {category} - {message}");
                break;
            case LogLevel.Warning:
                Warn($"{messageTime} {level}: {category} - {message}");
                break;
            case LogLevel.Error:
                Error($"{messageTime} {level}: {category} - {message}");
                break;
            case LogLevel.Critical:
                Error($"{messageTime} {level}: {category} - {message}");
                break;
        }
    }
    public void Log(LogLevel level, Exception err,
        [System.Runtime.CompilerServices.CallerFilePath] string? file = null)
        => Log(level, err.ToString(), file);
}