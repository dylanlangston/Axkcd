using VerifyTests.Http;

namespace AvaloniaXKCD.Tests;

public class XKCDClientTests
{
    [ClassDataSource<MockHttpClientHandler>(Shared = SharedType.None)]
    public required MockHttpClientHandler Handler { get; init; }

    // Mock JSON response for the latest comic.
    private const string LatestComicJson = """
    {
        "month": "10",
        "num": 2999,
        "link": "",
        "year": "2025",
        "news": "",
        "safe_title": "Fake Latest Comic",
        "transcript": "[[A fake comic for testing purposes.]]",
        "alt": "This is a test.",
        "img": "https://imgs.xkcd.com/comics/fake_latest.png",
        "title": "Fake Latest Comic",
        "day": "18"
    }
    """;

    // Mock JSON response for a specific, older comic.
    private const string SpecificComicJson = """
    {
        "month": "4",
        "num": 614,
        "link": "",
        "year": "2009",
        "news": "",
        "safe_title": "Woodpecker",
        "transcript": "[[A comic about a woodpecker.]]",
        "alt": "There's no wood, but it sounds like he's found a metal stud.",
        "img": "https://imgs.xkcd.com/comics/woodpecker.png",
        "title": "Woodpecker",
        "day": "22"
    }
    """;

    [Test]
    public async Task LatestShouldReturnTheLatestComic()
    {
        Handler?.AddMockedResponse(new(HttpMethod.Get, "/info.0.json"), new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(LatestComicJson)
        });

        var options = new XKCDClientOptions(Handler: Handler);
        using var client = new XKCDClient(options);

        var comic = await client.Latest();

        await Verify(comic)
            .Assert(_ =>
            {
                _.ShouldNotBeNull();
                _.Num.ShouldBeEquivalentTo(2999);
                _.Title.ShouldBeEquivalentTo("Fake Latest Comic");
            });
    }

    [Test]
    public async Task GetComicShouldReturnTheCorrectComicWhenFound()
    {
        Handler?.AddMockedResponse(new(HttpMethod.Get, "/614/info.0.json"), new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(SpecificComicJson)
        });

        var options = new XKCDClientOptions(Handler: Handler);
        using var client = new XKCDClient(options);

        var comic = await client.GetComic(614);

        await Verify(comic)
            .Assert(_ =>
            {
                _.ShouldNotBeNull();
                _.Num.ShouldBeEquivalentTo(614);
                _.Title.ShouldBeEquivalentTo("Woodpecker");
            });
    }

    [Test]
    public async Task GetComicShouldReturnLatestComicWhenComicIsNotFound()
    {
        Handler?.AddMockedResponse(new(HttpMethod.Get, "/9999/info.0.json"), new HttpResponseMessage(HttpStatusCode.NotFound));

        Handler?.AddMockedResponse(new(HttpMethod.Get, "/info.0.json"), new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(LatestComicJson)
        });

        var options = new XKCDClientOptions(Handler: Handler);
        using var client = new XKCDClient(options);

        var comic = await client.GetComic(9999);

        await Verify(comic)
            .Assert(_ =>
            {
                _.ShouldNotBeNull();
                _.Num.ShouldBeEquivalentTo(2999); // Should be the latest comic's number.
                _.Title.ShouldBeEquivalentTo("Fake Latest Comic");
            });
    }

    [Test]
    public async Task ConstructorShouldUseCustomBaseUriIfProvided()
    {
        Handler?.AddMockedResponse(new(HttpMethod.Get, "/info.0.json"), new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(LatestComicJson)
        });

        var customUri = new Uri("http://test.local/");
        var options = new XKCDClientOptions(BaseUri: customUri, Handler: Handler);
        using var client = new XKCDClient(options);

        await client.Latest();

        await Verify(Handler?.LastRequest!)
            .Assert(request =>
            {
                request.ShouldNotBeNull();
                request.RequestUri?.Host.ShouldBe("test.local");
                request.RequestUri?.Scheme.ShouldBe("http");
            });
    }

    [Test]
    public async Task ShouldUseAlternativeMirrorBaseUri()
    {
        var handler = new RecordingHttpClientHandler();
        var mirrorUri = new Uri("https://raw.githubusercontent.com/aghontpi/mirror-xkcd-api/main/api/");
        var options = new XKCDClientOptions(BaseUri: mirrorUri, Handler: handler);
        using var client = new XKCDClient(options);

        // Act
        var comic = await client.GetComic(300);

        // Assert
        await Verify(comic)
            .Assert(_ =>
            {
                _.ShouldNotBeNull();
                _.Num.ShouldBeEquivalentTo(300);
                _.Title.ShouldBeEquivalentTo("Facebook");
            })
            .UpdateSettings(_ => _.DisableRequireUniquePrefix());

        await Verify(handler?.LastRequest!)
            .UpdateSettings(settings => settings.UseFileName($"{nameof(XKCDClientTests)}.{nameof(ShouldUseAlternativeMirrorBaseUri)}.LastRequest"))
            .Assert(request =>
            {
                request.ShouldNotBeNull();
                request.RequestUri.ShouldNotBeNull();
                request.RequestUri!.Host.ShouldBe("raw.githubusercontent.com");
                request.RequestUri.AbsoluteUri.ShouldStartWith("https://raw.githubusercontent.com/aghontpi/mirror-xkcd-api/main/api/");
            })
            .UpdateSettings(_ => _.DisableRequireUniquePrefix());
    }

    [Test]
    public async Task AbleToInteractWithActualXKCDApi()
    {
        var options = new XKCDClientOptions(Handler: new RecordingHttpClientHandler());
        using var client = new XKCDClient(options);

        var comic = await client.Latest();

        await Verify(comic.BaseUri)
            .Assert(_ =>
            {
                _.ShouldNotBeNull();
            });
    }

    [Test]
    public async Task DisposeCanBeCalledMultipleTimesWithoutError()
    {
        var client = new XKCDClient();

        client.Dispose();

        await Should.NotThrowAsync(() =>
        {
            client.Dispose();
            return Task.CompletedTask;
        });
    }
}