using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace AvaloniaXKCD;

public record struct ParsedArguments(int? ComicNumber);

/// <summary>
/// Parses command-line arguments for the application using System.CommandLine.
/// </summary>
public class CommandLineParser
{
    private readonly RootCommand _rootCommand;
    private readonly Argument<int?> _comicNumberArgument;

    private CommandLineParser()
    {
        _comicNumberArgument = new Argument<int?>(name: "comic-number")
        {
            Description = "The specific comic number to load on startup.",
            DefaultValueFactory = (_) => null,
        };

        _rootCommand = new RootCommand("Avalonia XKCD Viewer") { _comicNumberArgument };
    }

    public static CommandLineParser Instance { get; } = new();

    public async Task<int> Invoke(
        string[] args,
        Func<ParsedArguments, int> action,
        CancellationToken cancellation = default
    )
    {
        _rootCommand.SetAction(parseResult =>
        {
            var comicNumber = parseResult.GetValue(_comicNumberArgument);
            var parsedArgs = new ParsedArguments(comicNumber);
            return action(parsedArgs);
        });
        var parseResult = _rootCommand.Parse(args);
        return await parseResult.InvokeAsync(cancellationToken: cancellation);
    }

    public async Task<int> Invoke(
        string[] args,
        Func<ParsedArguments, Task> action,
        CancellationToken cancellation = default
    )
    {
        _rootCommand.SetAction(parseResult =>
        {
            var comicNumber = parseResult.GetValue(_comicNumberArgument);
            var parsedArgs = new ParsedArguments(comicNumber);
            return action(parsedArgs);
        });
        var parseResult = _rootCommand.Parse(args);
        return await parseResult.InvokeAsync(cancellationToken: cancellation);
    }

    public async Task<int> Invoke(
        string[] args,
        Func<ParsedArguments, Task<int>> action,
        CancellationToken cancellation = default
    )
    {
        _rootCommand.SetAction(parseResult =>
        {
            var comicNumber = parseResult.GetValue(_comicNumberArgument);
            var parsedArgs = new ParsedArguments(comicNumber);
            return action(parsedArgs);
        });
        var parseResult = _rootCommand.Parse(args);
        return await parseResult.InvokeAsync(cancellationToken: cancellation);
    }
}
