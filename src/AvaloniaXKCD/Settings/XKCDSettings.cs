namespace AvaloniaXKCD.Settings;

public class XKCDSettings
{
    public XKCDSettings()
    {
        // Set default values
        if (OperatingSystem.IsBrowser())
        {

            BaseURL = new Uri(App.SystemActions.GetBaseUri(), "/mirror/");
        }
        else BaseURL = new Uri("https://xkcd.com/", UriKind.Absolute);
    }

    public Uri BaseURL { get; set; }
}

[JsonSerializable(typeof(XKCDSettings))]
internal partial class XKCDSettingsContext : JsonSerializerContext
{
}