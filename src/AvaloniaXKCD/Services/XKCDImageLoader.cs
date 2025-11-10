using System.Net.Http;
using AsyncImageLoader.Loaders;
using Avalonia.Media.Imaging;

namespace AvaloniaXKCD.Services;

[Service(Generators.ServiceLifetime.Singleton)]
public class XKCDImageLoader : RamCachedWebImageLoader
{
    public XKCDImageLoader(HttpClient httpClient)
        : base(httpClient, true)
    {
        App.Logger.LogInformation("XKCD Image Loader Loaded");
    }

    protected override async Task<Bitmap?> LoadAsync(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        var queryParams = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
        var comicNumber = int.Parse(queryParams["comic"]!);

        // Attempt to load larger image
        var img2x = IXKCDComic.GetImg2x(comicNumber, url);
        var bitmap = await base.LoadAsync(img2x);
        return bitmap ?? await base.LoadAsync(url);
    }
}