namespace AvaloniaXKCD.Tests.TestBases;

public abstract class BrowserBaseTest : PageTest
{
    private readonly string _browserName;

    protected BrowserBaseTest(string browser)
        : base(new BrowserTypeLaunchOptions
        {
            Timeout = 90_000, // 90 second timeout for browser launch
            Args =
            [
                // Flags for limited resource environments like CI
                "--disable-dev-shm-usage",
                "--no-sandbox",
                "--disable-setuid-sandbox"
            ]
        })
    {
        _browserName = browser;
    }

    public override string BrowserName => _browserName;

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
}