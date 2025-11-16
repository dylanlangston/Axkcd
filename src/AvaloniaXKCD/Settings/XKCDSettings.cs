namespace AvaloniaXKCD.Settings;

public class XKCDSettings
{
    static Uri GetMirrorUri(Uri uri)
    {
        var segments = uri.AbsolutePath.Trim('/').Split('/');
        if (segments.Length > 0 && !Int32.TryParse(segments[0], out var _))
        {
            var firstSegment = segments[0];
            var baseUrl = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}/{firstSegment}/";
            return new Uri($"{baseUrl}mirror/");
        }

        var rootUrl = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}/";
        return new Uri($"{rootUrl}mirror/");
    }

    public XKCDSettings()
    {
        // Set default values
        if (OperatingSystem.IsBrowser())
        {

            BaseURL = GetMirrorUri(App.SystemActions.GetBaseUri());
        }
        else BaseURL = new Uri("https://xkcd.com/", UriKind.Absolute);
    }

    public Uri BaseURL { get; set; }
}

[JsonSerializable(typeof(XKCDSettings))]
internal partial class XKCDSettingsContext : JsonSerializerContext
{
}