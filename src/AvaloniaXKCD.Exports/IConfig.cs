namespace AvaloniaXKCD.Exports;

public interface IConfig : IExport
{
    public PlatformType PlatformType { get; }

    public LogLevel LogLevel { get; }
}

public enum PlatformType
{
    Headless,
    Browser,
    Desktop
};