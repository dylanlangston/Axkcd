using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XKCDCore;

public record XKCDClientOptions(Uri? BaseUri = null, HttpMessageHandler? Handler = null);

public class XKCDClient : IDisposable
{
    private readonly HttpClient client;

    public XKCDClient(XKCDClientOptions? options = null)
    {
        var baseUri = options?.BaseUri ?? new Uri("https://xkcd.com/");
        client =
            options?.Handler != null ? new(options.Handler) { BaseAddress = baseUri } : new() { BaseAddress = baseUri };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("XKCDCore/1.0");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        TypeInfoResolver = XKCDJsonContext.Default,
    };

    public async Task<IXKCDComic> Latest()
    {
        var stream = await client.GetStreamAsync("info.0.json");
        var comic = await JsonSerializer.DeserializeAsync(stream, XKCDJsonContext.Default.XKCDComic);
        comic!.BaseUri = client.BaseAddress!;
        return comic;
    }

    public async Task<IXKCDComic> Random()
    {
        var latestComic = (await Latest()).Num;
        var comic = await GetComic(System.Random.Shared.Next(1, latestComic));
        return comic;
    }

    public async Task<IXKCDComic> GetComic(int number)
    {
        try
        {
            var stream = await client.GetStreamAsync($"{number}/info.0.json");
            var comic = await JsonSerializer.DeserializeAsync(stream, XKCDJsonContext.Default.XKCDComic);
            comic!.BaseUri = client.BaseAddress!;
            return comic;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return await Latest();
        }
    }

    #region IDisposable
    private bool disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
                client.Dispose();

            disposed = true;
        }
    }
    #endregion
}
