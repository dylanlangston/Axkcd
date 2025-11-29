using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.MarkupExtensions;

/// <summary>
/// Markup extension for localized strings that updates when the culture changes
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", 
        Justification = "The Binding source is a local private class (LocalizedStringProvider) where the property 'Value' is preserved via code structure or annotation.")]
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var localizationService = ExportContainer.Get<ILocalizationService>();
        if (localizationService == null)
        {
            return new Binding { Source = Key };
        }

        var binding = new Binding
        {
            Source = new LocalizedStringProvider(localizationService, Key),
            Path = nameof(LocalizedStringProvider.Value),
            Mode = BindingMode.OneWay
        };

        return binding;
    }

    private class LocalizedStringProvider : INotifyPropertyChanged
    {
        private readonly ILocalizationService _localizationService;
        private readonly string _key;

        public LocalizedStringProvider(ILocalizationService localizationService, string key)
        {
            _localizationService = localizationService;
            _key = key;
            _localizationService.CultureChanged += OnCultureChanged;
        }

        public string Value => _localizationService.GetString(_key);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnCultureChanged(object? sender, System.Globalization.CultureInfo e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}
