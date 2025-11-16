#!/usr/bin/dotnet run

// This script downloads all XKCD comics to a specified directory.
//
// DESIGN:
// The script uses a state machine pattern to manage the synchronization process, progressing through states like initialization, fetching the latest comic, processing individual comics in parallel, and finalization. It uses System.CommandLine for parsing command-line arguments and Spectre.Console for rich terminal output. Comic metadata is saved as 'info.0.json' and images are downloaded into numbered subdirectories. The script checks for existing files to avoid re-downloading and includes logic to handle corrupt local data.
//
// USAGE:
// Execute the script using 'dotnet run'.
//
// Required Arguments:
//   --output, -o <directory>    : The directory to save the XKCD comics.
//
// Optional Arguments:
//   --dop, -d <integer>         : The degree of parallelism for downloading. Defaults to the system's processor count.
//   --baseUrl, -b <uri>         : The base URL for the XKCD comics. Defaults to "https://xkcd.com/".
//
// Example:
//   dotnet run -- --output ./comics

#nullable enable
#:project ./XKCDCore
#:project ./AvaloniaXKCD.Utilities
#:package System.CommandLine
#:package Spectre.Console

using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvaloniaXKCD.Utilities;
using Spectre.Console;
using XKCDCore;

#region Handel Ctrl-C
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    AnsiConsole.MarkupLine("[bold red]Canceling...[/]");
    cts.Cancel();
    e.Cancel = true;
};
#endregion

#region CommandLine Options Setup
var outputOption = new Option<DirectoryInfo>(name: "--output", aliases: ["-o"])
{
    Description = "The directory to save the XKCD comics too.",
    Required = true,
};

var degreeOfParallelismOption = new Option<int>(name: "--dop", aliases: ["-d"])
{
    DefaultValueFactory = (_) => Environment.ProcessorCount,
    Description = "The degree of parallelism for downloading comics.",
};

var baseUrlOption = new Option<Uri>(name: "--baseUrl", aliases: ["-b"])
{
    DefaultValueFactory = (_) => new Uri("https://xkcd.com/"),
    Description = "The base URL for the XKCD comics.",
    CustomParser = result =>
    {
        var token = result.Tokens.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            result.AddError("Base URL is required.");
            return null!;
        }
        if (!Uri.TryCreate(token, UriKind.Absolute, out var uri))
        {
            result.AddError($"Invalid URL format: {token}");
            return null!;
        }
        return uri;
    },
};

var rootCommand = new RootCommand("XKCD Sync Tool") { outputOption, baseUrlOption, degreeOfParallelismOption };

rootCommand.SetAction(
    async (parseResult) =>
    {
        var output = parseResult.GetValue(outputOption);
        var baseUrl = parseResult.GetValue(baseUrlOption);
        var dop = parseResult.GetValue(degreeOfParallelismOption);
        await using var comicStaticMachine = new ComicSyncStateMachine(output, baseUrl, dop);
        await comicStaticMachine.RunAsync(cts.Token);
    }
);

ParseResult parseResult = rootCommand.Parse(args);

return await parseResult.InvokeAsync(cancellationToken: cts.Token);
#endregion

#region Sync Logic State Machine
public class ComicSyncStateMachine : IAsyncDisposable
{
    private readonly DirectoryInfo _outputDirectory;
    private readonly Uri _baseUrl;
    private SyncState? _currentState;
    private readonly HttpClientHandler _httpHandler = new();
    private readonly XKCDClient _xkcdClient;
    private readonly int _degreeOfParallelism;

    public ComicSyncStateMachine(DirectoryInfo? outputDirectory, Uri? baseUrl, int degreeOfParallelism)
    {
        outputDirectory.ThrowIfNull();
        baseUrl.ThrowIfNull();

        _outputDirectory = outputDirectory;
        _baseUrl = baseUrl;
        _xkcdClient = new XKCDClient(new() { Handler = _httpHandler });
        _currentState = new InitializeState();
        _degreeOfParallelism = degreeOfParallelism;
    }

    public ValueTask DisposeAsync()
    {
        _xkcdClient.Dispose();
        _httpHandler.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (_currentState != null)
        {
            _currentState = await _currentState.ExecuteAsync(this, cancellationToken);
        }
    }

    private abstract class SyncState
    {
        public abstract Task<SyncState?> ExecuteAsync(
            ComicSyncStateMachine context,
            CancellationToken cancellationToken
        );
    }

    private class InitializeState : SyncState
    {
        public override async Task<SyncState?> ExecuteAsync(
            ComicSyncStateMachine context,
            CancellationToken cancellationToken
        )
        {
            AnsiConsole.Write(new Rule("[bold blue]XKCD Sync Tool[/]").Centered());
            AnsiConsole.MarkupLine($"Output directory: [green]{context._outputDirectory.FullName}[/]");
            context._outputDirectory.Create();
            return new FetchLatestComicState();
        }
    }

    private class FetchLatestComicState : SyncState
    {
        public override async Task<SyncState?> ExecuteAsync(
            ComicSyncStateMachine context,
            CancellationToken cancellationToken
        )
        {
            var latestComic = await context._xkcdClient.Latest();
            AnsiConsole.MarkupLine($"Latest comic on server is [yellow]#{latestComic.Num}[/]");
            return new ProcessComicsState(latestComic.Num);
        }
    }

    private class ProcessComicsState : SyncState
    {
        private readonly int _latestComicNumber;

        public ProcessComicsState(int latestComicNumber)
        {
            _latestComicNumber = latestComicNumber;
        }

        public override async Task<SyncState?> ExecuteAsync(
            ComicSyncStateMachine context,
            CancellationToken cancellationToken
        )
        {
            var po = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = context._degreeOfParallelism,
            };

            await AnsiConsole
                .Progress()
                .Columns([
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn(),
                ])
                .StartAsync(async ctx =>
                {
                    var verificationTask = ctx.AddTask(
                        "[green]Verifying and downloading comics[/]",
                        new ProgressTaskSettings { MaxValue = _latestComicNumber }
                    );

                    await Parallel.ForEachAsync(
                        Enumerable.Range(1, _latestComicNumber),
                        po,
                        async (number, token) =>
                        {
                            token.ThrowIfCancellationRequested();
                            verificationTask.Description = $"Processing comic [blue]#{number}[/]";

                            if (number == 404) // comic #404 is not found ;p
                            {
                                verificationTask.Increment(1);
                                return;
                            }

                            try
                            {
                                await ProcessSingleComic(context, number, token);
                            }
                            catch (OperationCanceledException)
                            {
                                AnsiConsole.MarkupLine("[yellow]Download cancelled.[/]");
                                return;
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.MarkupLine($"[red]Failed to process comic #{number}: {ex.Message}[/]");
                            }
                            finally
                            {
                                verificationTask.Increment(1);
                            }
                        }
                    );
                });

            return new CopyLatestJsonState(_latestComicNumber);
        }

        private async Task ProcessSingleComic(
            ComicSyncStateMachine context,
            int number,
            CancellationToken cancellationToken
        )
        {
            var comicDirectory = context._outputDirectory.CreateSubdirectory(number.ToString());
            var jsonPath = Path.Combine(comicDirectory.FullName, "info.0.json");

            XKCDComic? comicToSave = null;
            IXKCDComic? remoteComic = null;
            bool needsSave = false;

            if (File.Exists(jsonPath))
            {
                using var fileStream = File.OpenRead(jsonPath);
                try
                {
                    comicToSave = (XKCDComic?)
                        await JsonSerializer.DeserializeAsync(
                            fileStream,
                            typeof(XKCDComic),
                            XKCDComicJsonSerializerContext.Default,
                            cancellationToken
                        );
                }
                catch (JsonException)
                {
                    AnsiConsole.MarkupLine($"[yellow]Local JSON for comic #{number} is corrupt. Re-downloading.[/]");
                }
            }

            if (comicToSave == null)
            {
                remoteComic = await context._xkcdClient.GetComic(number);
                needsSave = true;

                comicToSave = new XKCDComic(
                    Month: remoteComic.Month,
                    Num: remoteComic.Num,
                    Link: remoteComic.Link,
                    Year: remoteComic.Year,
                    News: remoteComic.News,
                    SafeTitle: remoteComic.SafeTitle,
                    Transcript: remoteComic.Transcript,
                    Alt: remoteComic.Alt,
                    Img: Path.Combine(remoteComic.Num.ToString(), Path.GetFileName(new Uri(remoteComic.Img).LocalPath)),
                    Title: remoteComic.Title,
                    Day: remoteComic.Day
                )
                {
                    BaseUri = context._baseUrl,
                };
            }
            else
            {
                // Validate that the existing comic has the correct Img and BaseURL
                if (Uri.TryCreate(comicToSave.Img, UriKind.Absolute, out var imgUri))
                {
                    comicToSave = comicToSave with
                    {
                        Img = Path.Combine(comicToSave.Num.ToString(), Path.GetFileName(imgUri.LocalPath)),
                    };
                    needsSave = true;
                }
                else if (
                    Path.Combine(comicToSave.Num.ToString(), Path.GetFileName(comicToSave.Img)) is string expectedImg
                    && comicToSave.Img != expectedImg
                )
                {
                    comicToSave = comicToSave with { Img = expectedImg };
                    needsSave = true;
                }
                if (comicToSave.BaseUri != context._baseUrl)
                {
                    comicToSave = comicToSave with { BaseUri = context._baseUrl };
                    needsSave = true;
                }
            }

            if (needsSave)
            {
                await File.WriteAllTextAsync(
                    jsonPath,
                    JsonSerializer.Serialize(comicToSave, typeof(XKCDComic), XKCDComicJsonSerializerContext.Default),
                    cancellationToken
                );
            }

            var localImgPath = Path.Combine(comicDirectory.FullName, Path.GetFileName(comicToSave.Img));
            bool needsImgDownload = !File.Exists(localImgPath);

            if (remoteComic == null && needsImgDownload)
            {
                remoteComic = await context._xkcdClient.GetComic(number);
            }

            if (needsImgDownload)
            {
                using var httpClient = new HttpClient(context._httpHandler, disposeHandler: false);
                var downloadSuccessful = await DownloadImageAsync(
                    httpClient,
                    new Uri(remoteComic!.Img),
                    comicDirectory,
                    comicToSave.Num,
                    cancellationToken: cancellationToken
                );
                if (downloadSuccessful)
                    await DownloadImageAsync(
                        httpClient,
                        new Uri(remoteComic!.Img2x),
                        comicDirectory,
                        comicToSave.Num,
                        is2x: true,
                        cancellationToken: cancellationToken
                    );
            }
        }

        async Task<bool> DownloadImageAsync(
            HttpClient client,
            Uri url,
            DirectoryInfo targetDir,
            int comic,
            bool is2x = false,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrEmpty(url.LocalPath) || url.LocalPath == "/")
                return false;

            var fileName = Path.GetFileName(url.LocalPath);
            var filePath = Path.Combine(targetDir.FullName, fileName);

            if (File.Exists(filePath))
            {
                return true;
            }

            try
            {
                var imageData = await client.GetByteArrayAsync(url, cancellationToken);
                await File.WriteAllBytesAsync(filePath, imageData, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]Cancellation token received. Halting download comic #{comic} for image {url}[/]"
                );
                throw;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]Could not download comic #{comic} image {(is2x ? "(2x) " : "")}{url}: {ex.Message}[/]"
                );
                return false;
            }
        }
    }

    private class CopyLatestJsonState : SyncState
    {
        private readonly int _latestComicNumber;

        public CopyLatestJsonState(int latestComicNumber)
        {
            _latestComicNumber = latestComicNumber;
        }

        public override Task<SyncState?> ExecuteAsync(
            ComicSyncStateMachine context,
            CancellationToken cancellationToken
        )
        {
            AnsiConsole.MarkupLine(
                $"Copying latest comic info ([yellow]#{_latestComicNumber}[/]) to root output directory..."
            );
            var sourcePath = Path.Combine(
                context._outputDirectory.FullName,
                _latestComicNumber.ToString(),
                "info.0.json"
            );
            var destPath = Path.Combine(context._outputDirectory.FullName, "info.0.json");

            if (File.Exists(sourcePath))
            {
                try
                {
                    File.Copy(sourcePath, destPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to copy latest comic info: {ex.Message}[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine(
                    $"[red]Could not find the source JSON file for the latest comic: {sourcePath}[/]"
                );
            }

            return Task.FromResult<SyncState?>(new FinalizeState());
        }
    }

    private class FinalizeState : SyncState
    {
        public override Task<SyncState?> ExecuteAsync(
            ComicSyncStateMachine context,
            CancellationToken cancellationToken
        )
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                AnsiConsole.MarkupLine("[bold green]Sync complete![/]");
            }
            return Task.FromResult<SyncState?>(null);
        }
    }
}
#endregion

#region --- Json Source Generation Context ---
[JsonSerializable(typeof(XKCDComic))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
)]
internal partial class XKCDComicJsonSerializerContext : JsonSerializerContext { }
#endregion
#nullable disable
