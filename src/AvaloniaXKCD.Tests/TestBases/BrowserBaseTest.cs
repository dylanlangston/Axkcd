namespace AvaloniaXKCD.Tests.TestBases;

public abstract class BrowserBaseTest(string browser) : PageTest, IAsyncDisposable
{
    public override string BrowserName => browser;

    [ClassDataSource<AvaloniaBrowserProject>(Shared = SharedType.PerAssembly)]
    public required AvaloniaBrowserProject AvaloniaManager { get; init; }

    public override BrowserNewContextOptions ContextOptions(TestContext testContext)
    {
        return new()
        {
            Locale = "en-US",
            ColorScheme = ColorScheme.Dark,
            BaseURL = AvaloniaManager.Url
        };
    }

    public async ValueTask DisposeAsync()
    {
        await AvaloniaManager.DisposeAsync();
    }
}