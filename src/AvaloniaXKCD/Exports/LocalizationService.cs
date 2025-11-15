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
    private CultureInfo _currentCulture;

    protected LocalizationService()
    {
        // Load resources from AvaloniaXKCD.Core assembly where the .resx files are embedded
        var resourceAssembly = System.Reflection.Assembly.Load("AvaloniaXKCD.Core");
        _resourceManager = new ResourceManager("AvaloniaXKCD.Resources.Strings", resourceAssembly);
        
        // Initialize with system culture or fallback to English
        _currentCulture = GetSystemCulture();
    }

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (!Equals(_currentCulture, value))
            {
                _currentCulture = value;
                CultureInfo.CurrentCulture = value;
                CultureInfo.CurrentUICulture = value;
                CultureChanged?.Invoke(this, value);
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
            // Fallback to English if culture not found
            SetCulture(CultureInfo.GetCultureInfo("en"));
        }
    }

    public string GetString(string key)
    {
        try
        {
            var value = _resourceManager.GetString(key, _currentCulture);
            return value ?? key;
        }
        catch
        {
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
        catch
        {
            return format;
        }
    }

    private static CultureInfo GetSystemCulture()
    {
        try
        {
            // Try to get the system's UI culture
            var systemCulture = CultureInfo.CurrentUICulture;
            
            // Check if we have resources for this culture or its parent
            var supportedCultures = new[] { "en", "es" };
            
            // Try the specific culture first (e.g., "es-MX")
            if (supportedCultures.Contains(systemCulture.Name))
            {
                return systemCulture;
            }
            
            // Try the neutral culture (e.g., "es" from "es-MX")
            if (supportedCultures.Contains(systemCulture.TwoLetterISOLanguageName))
            {
                return CultureInfo.GetCultureInfo(systemCulture.TwoLetterISOLanguageName);
            }
            
            // Fallback to English
            return CultureInfo.GetCultureInfo("en");
        }
        catch
        {
            // If all else fails, use English
            return CultureInfo.GetCultureInfo("en");
        }
    }
}
