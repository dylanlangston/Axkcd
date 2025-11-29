using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Themes.Fluent;

namespace AvaloniaXKCD.Tests.Orchestration;

public class HeadlessAvaloniaApp : AvaloniaXKCD.App
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<HeadlessAvaloniaApp>().UseSkia().UseHeadless(new() { UseHeadlessDrawing = false });

    public HeadlessAvaloniaApp() => Styles.Add(new FluentTheme());

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
