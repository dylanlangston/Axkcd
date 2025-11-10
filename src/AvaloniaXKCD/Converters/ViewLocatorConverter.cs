using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaXKCD.Converters;

public class ViewLocatorConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is IViewModelBase viewModel)
        {
            return ViewLocator.Instance.Build(viewModel);
        }
        return null;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is Control control && control.DataContext is IViewModelBase viewModel)
        {
            return viewModel;
        }
        return null;
    }
}