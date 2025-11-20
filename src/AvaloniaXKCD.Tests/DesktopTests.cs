namespace AvaloniaXKCD.Tests;

public class DesktopTests : AvaloniaBaseTest
{
    [Test]
    public Task LoadsInitialWindow()
    {
        return Verify<MainViewModel>().Assert<MainWindow>(_ => _.Title.ShouldBe("Axkcd"));
    }
}
