using System.Globalization;
using System.Resources;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Exports;

/// <summary>
/// Abstract base service for handling application localization using .resx files
/// </summary>
public abstract class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private CultureInfo _currentCulture = CultureInfo.GetCultureInfo("en");

    protected LocalizationService()
    {
        var resourceAssembly = typeof(LocalizationService).Assembly;
        _resourceManager = new ResourceManager("AvaloniaXKCD.Resources.Strings", resourceAssembly);

        CurrentCulture = GetCulture();
    }

    public static readonly string[] supportedCultures = new[] { "en", "es" };
    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (!Equals(_currentCulture, value))
            {
                void SetCultureInfo(CultureInfo culture)
                {
                    _currentCulture = culture;
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                    CultureChanged?.Invoke(this, culture);
                }

                try
                {
                    if (supportedCultures.Contains(value.Name))
                    {
                        SetCultureInfo(value);
                        return;
                    }

                    if (supportedCultures.Contains(value.TwoLetterISOLanguageName))
                    {
                        SetCultureInfo(CultureInfo.GetCultureInfo(value.TwoLetterISOLanguageName));
                        return;
                    }

                    SetCultureInfo(CultureInfo.GetCultureInfo("en"));
                }
                catch (Exception ex)
                {
                    App.Logger.LogWarning($"Failed to set culture: {ex.Message}");
                    SetCultureInfo(CultureInfo.GetCultureInfo("en"));
                }
            }
        }
    }

    public event EventHandler<CultureInfo>? CultureChanged;

    public void SetCulture(CultureInfo culture)
    {
        CurrentCulture = culture;
    }

    public void SetCulture(string languageCode)
    {
        try
        {
            var culture = CultureInfo.GetCultureInfo(languageCode);
            SetCulture(culture);
        }
        catch (CultureNotFoundException)
        {
            SetCulture(CultureInfo.GetCultureInfo("en"));
        }
    }

    public virtual string GetString(string key)
    {
        try
        {
            var value = _resourceManager.GetString(key, _currentCulture);
            App.Logger.LogDebug(
                $"LocalizationService: Retrieved string for key '{key}' in culture '{_currentCulture}': '{value}'"
            );
            return value ?? key;
        }
        catch (Exception ex)
        {
            App.Logger.LogWarning($"Failed to retrieve string for key '{key}': {ex.Message}");
            return key;
        }
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(format, args);
        }
        catch (Exception ex)
        {
            App.Logger.LogWarning($"Failed to format string '{format}' with args: {ex.Message}");
            return format;
        }
    }

    public abstract CultureInfo GetCulture();
}
