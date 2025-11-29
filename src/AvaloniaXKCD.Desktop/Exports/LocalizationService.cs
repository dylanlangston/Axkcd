using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Desktop;

/// <summary>
/// Desktop implementation of localization service
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public class DesktopLocalizationService : LocalizationService
{
    public override CultureInfo GetCulture()
        => CultureInfo.CurrentUICulture;
}
