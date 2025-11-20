using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using AvaloniaXKCD;
using AvaloniaXKCD.Browser;
using AvaloniaXKCD.Exports;

internal sealed partial class Program
{
    private static Task Main(string[] args) =>
        CommandLineParser
            .Instance.Invoke(
                args,
                async (parsedArguments) =>
                    await BuildAvaloniaApp(parsedArguments).WithInterFont().StartBrowserAppAsync("out")
            )
            .ContinueWith(result =>
            {
                if (result.IsFaulted)
                {
                    ExportContainer.Get<ISystemActions>()?.HandleError(result.Exception);
                    Environment.FailFast(result.Exception.ToString());
                }
            });

    public static AppBuilder BuildAvaloniaApp() => BuildAvaloniaApp(new ParsedArguments());

    public static AppBuilder BuildAvaloniaApp(ParsedArguments parsedArguments) =>
        AppBuilder.Configure(() => new App(parsedArguments));
}
