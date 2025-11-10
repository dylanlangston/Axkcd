namespace AvaloniaXKCD.Tests.Orchestration;

public partial class AvaloniaBrowserProject() : IAsyncInitializer, IAsyncDisposable
{
    string pathRelativeToSolution = "AvaloniaXKCD.Browser";

    private Process? _avaloniaProcess;
    private static readonly HttpClient HttpClient = new();
    private readonly TaskCompletionSource<string> _appUrlCompletionSource = new();

    string? _url;
    public string Url
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_url)) throw new NullReferenceException();
            return _url;
        }
        set => _url = value;
    }

    public async Task InitializeAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run",
            WorkingDirectory = @$"{Path.GetDirectoryName(TestContext.Current?.Metadata.TestDetails.TestFilePath)}/../{pathRelativeToSolution}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "true";

        _avaloniaProcess = new Process { StartInfo = startInfo };

        _avaloniaProcess.OutputDataReceived += OnOutputDataReceived;
        _avaloniaProcess.ErrorDataReceived += OnOutputDataReceived;

        try

        {
            _avaloniaProcess.Start();
            _avaloniaProcess.BeginOutputReadLine();
            _avaloniaProcess.BeginErrorReadLine();

            // Wait for the App url to be logged, with a decent timeout to allow for the build process.
            Url = await _appUrlCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(60));
            await PingUntilSuccess(Url);
        }
        catch
        {
            _avaloniaProcess.Kill(true);
            throw;
        }
        finally
        {
            HttpClient.Dispose();
        }

    }

    [GeneratedRegex(@"(http|https)://localhost:\d+", RegexOptions.Compiled)]
    private static partial Regex UrlRegex();

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        var match = UrlRegex().Match(e.Data);
        if (match.Success)
        {
            _appUrlCompletionSource.TrySetResult(match.Value);
        }
    }

    private static async Task PingUntilSuccess(string url)
    {
        var attempts = 0;
        const int maxAttempts = 5;

        while (attempts < maxAttempts && (TestContext.Current?.Execution.CancellationToken.IsCancellationRequested != true))
        {
            try
            {
                var response = await HttpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Attempt {attempts + 1} failed: {ex.Message}");
            }

            attempts++;
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"Failed to connect to the Avalonia application at {url} after {maxAttempts} attempts.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_avaloniaProcess is not null && !_avaloniaProcess.HasExited)
        {
            _avaloniaProcess.Kill(entireProcessTree: true); // Ensure child processes are also terminated
            await _avaloniaProcess.WaitForExitAsync();
        }

        _avaloniaProcess?.Dispose();
    }
}