using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace AvaloniaXKCD.Converters;

public class BitmapWidthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Bitmap bitmap)
        {
            return bitmap.Size.Width;
        }
        else if (value is null)
        {
            return value;
        }
        else if (value is string)
        {
            return null;
        }

        throw new NotImplementedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
