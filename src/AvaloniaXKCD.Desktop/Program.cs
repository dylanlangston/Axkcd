using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task<int> Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            cts.Cancel();
            e.Cancel = true;
        };

        return await CommandLineParser.Instance.Invoke(
            args,
            async (parsedArgs) =>
            {
                try
                {
                    return BuildAvaloniaApp(parsedArgs, cts.Token).StartWithClassicDesktopLifetime(args);
                }
                catch (Exception err)
                {
                    ExportContainer.Get<ISystemActions>()?.HandleError(err);
                    throw;
                }
            },
            cts.Token
        );
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(new ParsedArguments(), CancellationToken.None);

    public static AppBuilder BuildAvaloniaApp(ParsedArguments parsedArguments, CancellationToken cancellationToken) =>
        AppBuilder
            .Configure(() => new App(parsedArguments, cancellationToken))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
