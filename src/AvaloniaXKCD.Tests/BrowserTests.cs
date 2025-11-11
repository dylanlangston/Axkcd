namespace AvaloniaXKCD.Tests;

[Timeout(5 * 60 * 1000)] // 5 minutes
[Arguments("chromium")]
[Arguments("firefox")]
public class BrowserTests(string browser) : BrowserBaseTest(browser)
{
    [Test]
    public async Task LoadsInitialPage(CancellationToken cancellation)
    {
        await Page.GotoAsync("/");
        await Verify(Page)
            .UpdateSettings(_ =>
                _.PageScreenshotOptions(
                new()
                {
                    Quality = 50,
                    Type = ScreenshotType.Jpeg
                }, screenshotOnly: true))
            .Assert<IPage>(async _ =>
            {
                await Expect(Page).ToHaveTitleAsync(new Regex(@"^(A\(valonia\)XKCD|AXKCD: .*)$"));
            });
    }
}