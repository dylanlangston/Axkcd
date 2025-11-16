using System.Net;
using VerifyTests.Http;
using XKCDCore; // Ensure this using directive points to your project's namespace

namespace AvaloniaXKCD.Tests;

public class ExplainXKCDTests
{
    // Injects a fresh MockHttpClientHandler for each test.
    [ClassDataSource<MockHttpClientHandler>(Shared = SharedType.None)]
    public required MockHttpClientHandler Handler { get; init; }

    // Mock JSON response for a successful API call to get an explanation.
    private const string ExplanationApiResponseJson = """
        {
            "batchcomplete": "",
            "query": {
                "redirects": [
                    {
                        "from": "74",
                        "to": "74: Hell"
                    }
                ],
                "pages": {
                    "661": {
                        "pageid": 661,
                        "ns": 0,
                        "title": "74: Hell",
                        "revisions": [
                            {
                                "*": "'''[[74: Hell]]''' is a comic about a fictional depiction of hell.\n\n== Explanation ==\nThis comic plays on the tropes of eternal damnation.\n\n* The first panel shows a classic fire-and-brimstone scene.\n* The second introduces a twist: boring meetings.\n\nThis references [[Category:Christianity]] and the concept of [[Sin|sin]].\n\nA link to Wikipedia about {{w|Dante's Inferno}}.\n\nHere is a reference to another comic: [[614: Woodpecker]].\n\n== Transcript =="
                            }
                        ]
                    }
                }
            }
        }
        """;

    // Mock JSON response for an API call where the page is empty or not found.
    private const string EmptyApiResponseJson = """
        {
            "batchcomplete": "",
            "query": {
                "pages": {
                    "-1": {
                        "ns": 0,
                        "title": "9999",
                        "missing": ""
                    }
                }
            }
        }
        """;

    [Test]
    public async Task GetExplanation_ShouldReturnCorrectlyParsedHtml_WhenComicIsFound()
    {
        // Arrange
        var requestUri = "/wiki/api.php?action=query&prop=revisions&rvprop=content&format=json&redirects=1&titles=74";
        Handler.AddMockedResponse(
            new(HttpMethod.Get, requestUri),
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ExplanationApiResponseJson),
            }
        );

        var options = new ExplainXKCDClientOptions(Handler: Handler);
        using var client = new ExplainXKCDClient(options);

        // Act
        var explanationHtml = await client.GetExplanation(74);

        // Assert
        await VerifyAssertionsPlugin
            .Verify(explanationHtml)
            .Assert(html =>
            {
                html.ShouldNotBeNullOrEmpty();
                html.ShouldStartWith("<p>This comic plays on the tropes of eternal damnation.</p>");
                html.ShouldContain("<li>The first panel shows a classic fire-and-brimstone scene.</li>");
                html.ShouldContain(
                    "<a href=\"https://en.wikipedia.org/wiki/Dante%27s%20Inferno\" title=\"wikipedia:Dante&#39;s Inferno\">Dante&#39;s Inferno</a>"
                );
                html.ShouldContain("<a href=\"https://xkcd.com/614\">614: Woodpecker</a>");
            });
    }

    [Test]
    public async Task GetExplanation_ShouldReturnNotFoundMessage_WhenComicDoesNotExist()
    {
        // Arrange
        var requestUri = "/wiki/api.php?action=query&prop=revisions&rvprop=content&format=json&redirects=1&titles=9999";
        Handler.AddMockedResponse(
            new(HttpMethod.Get, requestUri),
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(EmptyApiResponseJson),
            }
        );

        var options = new ExplainXKCDClientOptions(Handler: Handler);
        using var client = new ExplainXKCDClient(options);

        // Act
        var result = await client.GetExplanation(9999);

        // Assert
        await VerifyAssertionsPlugin
            .Verify(result)
            .Assert(text =>
            {
                text.ShouldBe("Could not find an explanation for this comic.");
            });
    }

    [Test]
    public async Task GetExplanation_ShouldReturnNetworkErrorMessage_WhenApiRequestFails()
    {
        // Arrange
        var requestUri = "/wiki/api.php?action=query&prop=revisions&rvprop=content&format=json&redirects=1&titles=123";
        Handler.AddMockedResponse(
            new(HttpMethod.Get, requestUri),
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
        );

        var options = new ExplainXKCDClientOptions(Handler: Handler);
        using var client = new ExplainXKCDClient(options);

        // Act
        var result = await client.GetExplanation(123);

        // Assert
        await VerifyAssertionsPlugin
            .Verify(result)
            .Assert(text =>
            {
                text.ShouldBe("Could not find an explanation for this comic (network error).");
            });
    }

    [Test]
    public async Task ConstructorShouldUseCustomBaseUriIfProvided()
    {
        // Arrange
        var requestUri = "/wiki/api.php?action=query&prop=revisions&rvprop=content&format=json&redirects=1&titles=1";
        Handler.AddMockedResponse(
            new(HttpMethod.Get, requestUri),
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(ExplanationApiResponseJson),
            }
        );

        var customUri = new Uri("http://test.local/");
        var options = new ExplainXKCDClientOptions(BaseUri: customUri, Handler: Handler);
        using var client = new ExplainXKCDClient(options);

        // Act
        await client.GetExplanation(1);

        // Assert
        await Verify(Handler.LastRequest!)
            .Assert(request =>
            {
                request.ShouldNotBeNull();
                request.RequestUri?.Host.ShouldBe("test.local");
                request.RequestUri?.Scheme.ShouldBe("http");
            });
    }

    [Test]
    public async Task GetExplanation_ShouldReturnValidHtml_FromRealApi()
    {
        // Arrange: Use a real HttpClient by not providing a DelegatingHandler.
        // This test requires an active internet connection.
        using var client = new ExplainXKCDClient();

        // Act: Get the explanation for a known, stable comic (614: Woodpecker).
        var explanationHtml = await client.GetExplanation(614);

        // Assert
        await VerifyAssertionsPlugin
            .Verify(explanationHtml)
            .Assert(html =>
            {
                html.ShouldNotBeNullOrEmpty();
                html.ShouldContain("Beret Guy", Case.Insensitive);
                html.ShouldContain("woodpecker", Case.Insensitive);
                html.ShouldContain("power drill", Case.Insensitive);
                // Verify that a wikitext link was parsed correctly.
                html.ShouldContain("<a href=\"https://en.wikipedia.org/wiki/woodpecker\"");
            });
    }

    [Test]
    public async Task DisposeCanBeCalledMultipleTimesWithoutError()
    {
        // Arrange
        var client = new ExplainXKCDClient();

        // Act
        client.Dispose();

        // Assert
        await Should.NotThrowAsync(() =>
        {
            client.Dispose();
            return Task.CompletedTask;
        });
    }
}
