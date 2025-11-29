using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.MarkupExtensions;

/// <summary>
/// Markup extension for localized strings that updates when the culture changes
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var localizationService = ExportContainer.Get<ILocalizationService>();
        if (localizationService == null)
            return Key;

        var provider = new LocalizedStringProvider(localizationService, Key);

        // Create an IPropertyInfo for the 'Value' CLR property
        var propInfo = new ClrPropertyInfo(
            name: nameof(LocalizedStringProvider.Value),
            getter: obj => ((LocalizedStringProvider)obj!).Value,
            setter: null,
            propertyType: typeof(string)
        );

        // Build a compiled binding path that uses an INPC-aware accessor factory
        var path = new CompiledBindingPathBuilder()
            .Property(
                propInfo,
                // Use Avalonia's helper to create an INPC-capable property accessor
                PropertyInfoAccessorFactory.CreateInpcPropertyAccessor
            )
            .Build();

        // Create and return a CompiledBindingExtension using that compiled path
        var ext = new CompiledBindingExtension(path)
        {
            Source = provider,
            Mode = BindingMode.OneWay
        };

        return ext.ProvideValue(serviceProvider);
    }

    private sealed class LocalizedStringProvider : ObservableObject, IDisposable
    {
        private readonly ILocalizationService _service;
        private readonly string _key;

        public LocalizedStringProvider(ILocalizationService service, string key)
        {
            _service = service;
            _key = key;
            _service.CultureChanged += OnCultureChanged;
        }

        public string Value => _service.GetString(_key);

        private void OnCultureChanged(object? sender, CultureInfo e) => OnPropertyChanged(nameof(Value));

        public void Dispose()
        {
            _service.CultureChanged -= OnCultureChanged;
        }
    }
}
