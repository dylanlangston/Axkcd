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
    internal static partial void SetBrowserLocale(string locale);

    public BrowserLocalizationService()
    {
        // Try to get locale from browser
        try
        {
            var browserLocale = GetBrowserLocale();
            if (!string.IsNullOrEmpty(browserLocale))
            {
                SetCulture(browserLocale);
            }
        }
        catch
        {
            // If browser locale detection fails, use system default
        }

        // Listen for culture changes and sync to browser
        CultureChanged += OnCultureChangedHandler;
    }

    private void OnCultureChangedHandler(object? sender, CultureInfo culture)
    {
        try
        {
            SetBrowserLocale(culture.TwoLetterISOLanguageName);
        }
        catch
        {
            // Ignore errors syncing to browser
        }
    }
}
