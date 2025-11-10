using System.Runtime.CompilerServices;

namespace AvaloniaXKCD.Tests.VerifyPlugins;

public static class VerifyHeadlessAvaloniaPlugin
{
    public static AssertionVerificationTask<MainWindow> Verify<TViewModel>(VerifySettings? settings = null)
        where TViewModel : new()
    {
        var window = new MainWindow
        {
            DataContext = new TViewModel()
        };

        var verifier = Verifier.Verify(window, settings);

        return new AssertionVerificationTask<MainWindow>(window, verifier);
    }
}