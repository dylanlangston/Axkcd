using System;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Browser;

/// <summary>
/// Browser implementation of localization service with JavaScript interop
/// </summary>
public partial class BrowserLocalizationService : LocalizationService
{
    [JSImport("getLocale", "interop")]
    internal static partial string GetBrowserLocale();

    [JSImport("setLocale", "interop")]
    internal static partial Task SetBrowserLocale(string locale);

    [JSImport("getString", "interop")]
    internal static partial string GetBrowserString(string key);

    public override CultureInfo GetCulture()
        => GetBrowserLocale() switch
        {
            null or "" => CultureInfo.GetCultureInfo("en"),
            var locale => CultureInfo.GetCultureInfo(locale)
        };

    public override string GetString(string key)
    {
        return GetBrowserString(key);
    }

    public BrowserLocalizationService() : base()
    {
        CultureChanged += OnCultureChangedHandler;
    }

    private async void OnCultureChangedHandler(object? sender, CultureInfo culture)
    {
        try
        {
            await SetBrowserLocale(culture.TwoLetterISOLanguageName);
        }
        catch (Exception ex)
        {
            App.Logger.LogError($"Failed to set browser locale: {ex}");
        }
    }
}
