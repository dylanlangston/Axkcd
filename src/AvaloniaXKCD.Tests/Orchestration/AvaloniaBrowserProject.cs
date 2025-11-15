using AvaloniaXKCD.Utilities;

namespace AvaloniaXKCD.Tests.Orchestration;

public partial class AvaloniaBrowserProject() : IAsyncInitializer, IAsyncDisposable
{
    const string pathRelativeToSolution = "AvaloniaXKCD.Browser";
    static TimeSpan buildTimeout = TimeSpan.FromMinutes(3);

    private Process? _avaloniaProcess;
    private static readonly HttpClient HttpClient = new();
    private readonly TaskCompletionSource<string> _appUrlCompletionSource = new();
    private readonly List<string> _standardOutput = new();
    private readonly List<string> _standardError = new();

    string? _url;
    public string Url
    {
        get
        {
            _url.ThrowIfNull();
            return _url;
        }
        set => _url = value;
    }

    public async Task InitializeAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --no-launch-profile --no-restore",
            WorkingDirectory = @$"{Path.GetDirectoryName(TestContext.Current?.Metadata.TestDetails.TestFilePath)}/../{pathRelativeToSolution}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = "http://localhost:5000";

        _avaloniaProcess = new Process { StartInfo = startInfo };

        _avaloniaProcess.OutputDataReceived += OnOutputDataReceived;
        _avaloniaProcess.ErrorDataReceived += OnErrorDataReceived;

        try
        {
            _avaloniaProcess.Start();
            _avaloniaProcess.BeginOutputReadLine();
            _avaloniaProcess.BeginErrorReadLine();

            Url = await _appUrlCompletionSource.Task.WaitAsync(buildTimeout);

            if (_avaloniaProcess.HasExited) throw new Exception("Exited prematurely!");

            await PingUntilSuccess(Url);
        }
        catch (Exception ex)
        {
            Action<string?> writeAction = TestContext.Current != null ?
                (string? value) => TestContext.Current.OutputWriter.WriteLine(value) : 
                (string? value) => Console.WriteLine(value);
            writeAction("--- Avalonia App startup failed. Captured output: ---");
            writeAction("--- Standard Output: ---");
            _standardOutput.ForEach(writeAction);
            writeAction("--- Standard Error: ---");
            _standardError.ForEach(writeAction);

            if (_avaloniaProcess is not null && !_avaloniaProcess.HasExited)
            {
                _avaloniaProcess.Kill(true);
            }
            throw new InvalidOperationException("Failed to initialize the Avalonia application.", ex);
        }
        finally
        {
            HttpClient.Dispose();
        }

    }

    [GeneratedRegex(@"(http|https)://localhost:\d+", RegexOptions.Compiled)]
    private static partial Regex UrlRegex();

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            _standardError.Add(e.Data);
            _appUrlCompletionSource.TrySetException(new Exception(e.Data));
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Data))
        {
            return;
        }

        _standardOutput.Add(e.Data);
        var match = UrlRegex().Match(e.Data);
        if (match.Success)
        {
            _appUrlCompletionSource.TrySetResult(match.Value);
        }
    }

    private static async Task PingUntilSuccess(string url)
    {
        var attempts = 0;
        const int maxAttempts = 36;

        while (attempts < maxAttempts && (TestContext.Current?.Execution.CancellationToken.IsCancellationRequested != true))
        {
            try
            {
                var response = await HttpClient.GetAsync($"{url.TrimEnd('/')}/index.html");
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
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        throw new TimeoutException($"Failed to connect to the Avalonia application at {url} after {maxAttempts} attempts.");
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_avaloniaProcess is not null && !_avaloniaProcess.HasExited)
            {
                _avaloniaProcess.Kill(entireProcessTree: true); // Ensure child processes are also terminated
                await _avaloniaProcess.WaitForExitAsync();
            }
        }
        catch
        {

        }
        finally
        {
            _avaloniaProcess?.Dispose();
        }
    }
}