using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Tests.Exports;

/// <summary>
/// Test implementation of LocalizationService for unit tests.
/// This is in the Tests.Exports namespace to be properly registered.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
public sealed class TestLocalizationService : LocalizationService
{
    public override CultureInfo GetCulture()
        => CultureInfo.CurrentUICulture;
}
