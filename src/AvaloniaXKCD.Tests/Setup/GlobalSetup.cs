using System.Runtime.CompilerServices;
using AvaloniaXKCD.Tests.VerifyPlugins;

[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

[assembly: Timeout(30_000)]

namespace AvaloniaXKCD.Tests.Setup;

public class GlobalHooks
{
    [ModuleInitializer]
    public static void Init()
    {
        // Assertions
        VerifyAssertionsPlugin.Initialize();
        // HTTP
        VerifyHttp.Initialize();
        // Source Generators
        VerifySourceGenerators.Initialize();
        // Playwright
        VerifyPlaywright.Initialize(installPlaywright: true);
        if (Debugger.IsAttached)
        {
            // Debug playwright
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }
        // Headless Avalonia
        VerifyImageMagick.RegisterComparers(.24);
        VerifyImageMagick.Initialize();
        VerifyAvalonia.Initialize();
    }

    [Before(TestSession)]
    public static void SetUp()
    {
        Verifier.UseProjectRelativeDirectory("Verify/Outputs");
    }

    // [After(TestSession)]
    // public static void CleanUp()
    // {
    // }
}
