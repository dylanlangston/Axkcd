using Microsoft.Playwright;
using System.Text.RegularExpressions;
using AvaloniaXKCD.Tests.VerifyPlugins;

namespace AvaloniaXKCD.Tests;

/// <summary>
/// Browser tests for localization functionality using screenshot-based validation.
/// Due to Avalonia browser implementation (https://github.com/AvaloniaUI/Avalonia/issues/15453),
/// we validate localized UI by taking screenshots of the Avalonia canvas rather than querying DOM.
/// </summary>
[ParallelLimiter<BrowserLocalizationParallelLimit>]
[Timeout(5 * 60 * 1000)] // 5 minutes
[Arguments("chromium", "en-US")]
[Arguments("chromium", "es-ES")]
[Arguments("firefox", "en-US")]
[Arguments("firefox", "es-ES")]
public class BrowserLocalizationTests(string browser, string locale) : BrowserBaseTest(browser)
{
    public override BrowserNewContextOptions ContextOptions(TestContext testContext)
    {
        return new()
        {
            Locale = locale,
            ColorScheme = ColorScheme.Dark,
            BaseURL = AvaloniaManager.Url
        };
    }

    [Test]
    public async Task ShouldRenderLocalizedUIInCanvas(CancellationToken cancellation)
    {
        await Page.GotoAsync("/");
        
        // Wait for the Avalonia canvas to load and render
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for the canvas element to be present
        var canvas = Page.Locator("canvas");
        await Expect(canvas).ToBeVisibleAsync(new() { Timeout = 30000 });
        
        // Give the Avalonia app time to initialize and render localized content
        await Page.WaitForTimeoutAsync(3000);
        
        // Take a screenshot of the canvas showing localized UI controls
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
                // Verify page loaded successfully
                await Expect(Page).ToHaveTitleAsync(new Regex(@"^(A\(valonia\)XKCD|AXKCD: .*)$"));
                
                // Verify canvas is visible and has content
                await Expect(canvas).ToBeVisibleAsync();
            });
    }

    [Test]
    public async Task ShouldDetectBrowserLocale(CancellationToken cancellation)
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Get the detected locale from the browser via JavaScript
        var detectedLocale = await Page.EvaluateAsync<string>("() => navigator.language");

        // Verify the detected locale matches what we set
        await VerifyAssertionsPlugin.Verify(new { DetectedLocale = detectedLocale, ExpectedLocale = locale })
            .Assert(result =>
            {
                result.DetectedLocale.ShouldNotBeNullOrEmpty();
                // Browser might return more specific locale like "en-US" when we expect "en"
                result.DetectedLocale.ShouldStartWith(result.ExpectedLocale.Split('-')[0]);
            });
    }

    [Test]
    public async Task ShouldHaveLocalizedWindowTitle(CancellationToken cancellation)
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for app to initialize
        await Page.WaitForTimeoutAsync(2000);
        
        // Get the page title
        var title = await Page.TitleAsync();

        var result = new
        {
            Locale = locale,
            Title = title
        };

        // Verify title follows expected format
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(r =>
            {
                r.Title.ShouldNotBeNullOrEmpty();
                // Title should match the pattern for AXKCD
                // Either "A(valonia)XKCD" during loading or "AXKCD: {comic title}" when loaded
                var titlePattern = new Regex(@"^(A\(valonia\)XKCD|AXKCD: .*)$");
                titlePattern.IsMatch(r.Title).ShouldBeTrue($"Title '{r.Title}' should match AXKCD pattern");
            });
    }

    [Test]
    public async Task ShouldCallGetLocaleInterop(CancellationToken cancellation)
    {
        await Page.GotoAsync("/");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Call getLocale via JavaScript
        var browserLocale = await Page.EvaluateAsync<string>(@"
            async () => {
                const { getLocale } = await import('./interop.js');
                return getLocale();
            }
        ");

        var result = new
        {
            ExpectedLocale = locale,
            BrowserLocale = browserLocale
        };

        await VerifyAssertionsPlugin.Verify(result)
            .Assert(r =>
            {
                r.BrowserLocale.ShouldNotBeNullOrEmpty();
                // Browser locale should start with the language code
                r.BrowserLocale.ShouldStartWith(r.ExpectedLocale.Split('-')[0]);
            });
    }
}

public record BrowserLocalizationParallelLimit : IParallelLimit
{
    public int Limit => 1;
}