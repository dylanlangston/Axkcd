using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace XKCDCore;

public record ExplainXKCDClientOptions(
    Uri? BaseUri = null,
    HttpMessageHandler? Handler = null
);

public class ExplainXKCDClient : IDisposable
{
    private readonly HttpClient client;

    public ExplainXKCDClient(ExplainXKCDClientOptions? options = null)
    {
        var baseUri = options?.BaseUri ?? new Uri("https://www.explainxkcd.com/");
        client = options?.Handler != null
            ? new(options.Handler) { BaseAddress = baseUri }
            : new() { BaseAddress = baseUri };

        client.DefaultRequestHeaders.UserAgent.ParseAdd("XKCDCore/1.0");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> GetExplanation(IXKCDComic comic)
    {
        return await GetExplanation(comic.Num);
    }

    public async Task<string> GetExplanation(int comicNumber)
    {
        try
        {
            var requestUri = $"wiki/api.php?action=query&prop=revisions&rvprop=content&format=json&redirects=1&titles={comicNumber}";
            var response = await client.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                return "Could not find an explanation for this comic (network error).";
            }

            var content = await response.Content.ReadAsStringAsync();
            var mediaWikiResponse = JsonSerializer.Deserialize<MediaWikiResponse>(content, SerializerOptions);

            var page = mediaWikiResponse?.Query?.Pages?.Values.FirstOrDefault();
            var wikiContent = page?.Revisions?.FirstOrDefault()?.Content;

            if (string.IsNullOrWhiteSpace(wikiContent))
            {
                return "Could not find an explanation for this comic.";
            }

            var start = -1;
            var startMarker = wikiContent.IndexOf("{{incomplete|", StringComparison.OrdinalIgnoreCase);

            if (startMarker != -1)
            {
                start = wikiContent.IndexOf('\n', startMarker);
                if (start != -1) start++;
            }
            else
            {
                startMarker = wikiContent.IndexOf("== Explanation ==", StringComparison.OrdinalIgnoreCase);
                if (startMarker == -1) startMarker = wikiContent.IndexOf("==Explanation==", StringComparison.OrdinalIgnoreCase);

                if (startMarker != -1)
                {
                    start = wikiContent.IndexOf('\n', startMarker);
                    if (start != -1) start++;
                }
            }

            if (start == -1)
            {
                return "Could not find the start of the explanation section.";
            }

            var endMarker = wikiContent.IndexOf("==Transcript==", start, StringComparison.OrdinalIgnoreCase);
            if (endMarker == -1) endMarker = wikiContent.IndexOf("== Transcript ==", start, StringComparison.OrdinalIgnoreCase);
            if (endMarker == -1) endMarker = wikiContent.Length;

            var explanationWikitext = wikiContent.Substring(start, endMarker - start).Trim();

            return WikiTextParser.Instance.Parse(explanationWikitext, comicNumber);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
        {
            return $"Error fetching or parsing explanation: {ex.Message}";
        }
    }

    #region IDisposable
    private bool disposed;

    public void Dispose()
    {
        Dispose(true);
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

// Classes to deserialize the MediaWiki API response
public class MediaWikiResponse
{
    public Query? Query { get; set; }
}

public class Query
{
    public Dictionary<string, Page>? Pages { get; set; }
}

public class Page
{
    public int PageId { get; set; }
    public int Ns { get; set; }
    public string? Title { get; set; }
    public List<Revision>? Revisions { get; set; }
}

public class Revision
{
    [JsonPropertyName("*")]
    public string? Content { get; set; }
}