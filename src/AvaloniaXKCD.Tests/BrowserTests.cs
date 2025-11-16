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

        // Wait for the canvas element to be present
        var canvas = Page.Locator("canvas");
        await Expect(canvas).ToBeVisibleAsync(new() { Timeout = 30000 });

        // Give the Avalonia app time to initialize and render localized content
        await Page.WaitForTimeoutAsync(3000);

        await Verify(Page)
            .UpdateSettings(_ =>
                _.PageScreenshotOptions(
                new()
                {
                    Quality = 50,
                    Type = ScreenshotType.Jpeg,
                    Mask = [
                        canvas,
                    ]
                }, screenshotOnly: true)
                .ImageMagickComparer())
            .Assert<IPage>(async _ =>
            {
                await Expect(Page).ToHaveTitleAsync(new Regex(@"^(A\(valonia\)XKCD|AXKCD: .*)$"));
            });
    }
}
