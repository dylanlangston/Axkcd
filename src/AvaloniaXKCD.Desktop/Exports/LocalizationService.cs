using System.Globalization;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Desktop;

/// <summary>
/// Desktop implementation of localization service
/// </summary>
public class DesktopLocalizationService : LocalizationService
{
    public override CultureInfo GetCulture()
        => CultureInfo.CurrentUICulture;
}
