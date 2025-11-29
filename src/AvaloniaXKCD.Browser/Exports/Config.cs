using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Browser;

public class Config : IConfig
{
    public PlatformType PlatformType
    {
        get => PlatformType.Browser;
    }

#if DEBUG
    public LogLevel LogLevel
    {
        get => LogLevel.Trace;
    }
#else
    public LogLevel LogLevel
    {
        get => LogLevel.Critical;
    }
#endif
}
