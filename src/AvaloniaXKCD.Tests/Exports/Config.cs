namespace AvaloniaXKCD.Tests;

public class Config : IConfig
{
    public PlatformType PlatformType
    {
        get => PlatformType.Headless;
    }

    public LogLevel LogLevel
    {
        get => LogLevel.Trace;
    }
}
