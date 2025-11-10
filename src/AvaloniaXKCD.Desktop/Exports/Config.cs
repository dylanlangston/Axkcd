using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Desktop;

public class Config : IConfig
{
    public PlatformType PlatformType { get => PlatformType.Desktop; }

#if DEBUG
    public LogLevel LogLevel { get => LogLevel.Trace; }
#else
        public LogLevel LogLevel { get => LogLevel.Critical; }
#endif
}