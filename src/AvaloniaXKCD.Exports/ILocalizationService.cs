using System.Globalization;

namespace AvaloniaXKCD.Exports;

/// <summary>
/// Service for handling application localization
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current culture
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Event raised when the culture changes
    /// </summary>
    event EventHandler<CultureInfo>? CultureChanged;

    /// <summary>
    /// Sets the current culture
    /// </summary>
    /// <param name="culture">The culture to set</param>
    void SetCulture(CultureInfo culture);

    /// <summary>
    /// Sets the current culture by language code
    /// </summary>
    /// <param name="languageCode">The language code (e.g., "en", "es")</param>
    void SetCulture(string languageCode);

    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <returns>The localized string</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a formatted localized string by key
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="args">Format arguments</param>
    /// <returns>The formatted localized string</returns>
    string GetString(string key, params object[] args);
}
