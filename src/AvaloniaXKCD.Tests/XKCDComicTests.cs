namespace AvaloniaXKCD.Tests;

public class XKCDComicTests
{
    [Test]
    public async Task GetImg2xShouldReturn2xUrlForComics1084AndLater()
    {
        var pngUrl = "https://imgs.xkcd.com/comics/test.png";
        var jpgUrl = "https://imgs.xkcd.com/comics/another.jpg";
        var comicNumber = 1084;

        var pngResult = IXKCDComic.GetImg2x(comicNumber, pngUrl);
        var jpgResult = IXKCDComic.GetImg2x(comicNumber, jpgUrl);

        await Verify(new { pngResult, jpgResult })
            .Assert(results =>
            {
                results.pngResult.ShouldBe("https://imgs.xkcd.com/comics/test_2x.png");
                results.jpgResult.ShouldBe("https://imgs.xkcd.com/comics/another_2x.jpg");
            });
    }

    [Test]
    public async Task GetImg2xShouldReturnOriginalUrlForComicsBefore1084()
    {
        var originalUrl = "https://imgs.xkcd.com/comics/test.png";
        var comicNumber = 1083;

        var result = IXKCDComic.GetImg2x(comicNumber, originalUrl);

        await Verifier.Verify(result).Assert<string>(_ => _.ShouldBe(originalUrl));
    }

    [Test]
    public async Task GetImg2xShouldHandleEmptyUrl()
    {
        var result = IXKCDComic.GetImg2x(2000, string.Empty);

        await Verifier.Verify(result).Assert<string>(_ => _.ShouldBeEmpty());
    }

    [Test]
    public async Task GetURIShouldReturnCorrectComicLinkWithDefaultBaseUri()
    {
        IXKCDComic comic = new XKCDComic(
            Month: "10",
            Num: 404,
            Link: "",
            Year: "2025",
            News: "",
            SafeTitle: "Test",
            Transcript: "",
            Alt: "",
            Img: "",
            Title: "Test",
            Day: "18"
        );

        var uri = comic.GetURI();

        await Verify(uri).Assert(_ => _.AbsoluteUri.ShouldBe("https://xkcd.com/404/"));
    }

    [Test]
    public async Task GetURIShouldUseProvidedBaseUriOverride()
    {
        IXKCDComic comic = new XKCDComic(
            Month: "10",
            Num: 123,
            Link: "",
            Year: "2025",
            News: "",
            SafeTitle: "Test",
            Transcript: "",
            Alt: "",
            Img: "",
            Title: "Test",
            Day: "18"
        );
        var overrideUri = new Uri("http://localhost:8080/");

        var uri = comic.GetURI(overrideUri);

        await Verify(uri).Assert(_ => _.AbsoluteUri.ShouldBe("http://localhost:8080/123/"));
    }
}
