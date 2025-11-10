using System.Globalization;
using System.Numerics;
using Avalonia.Data.Converters;

namespace AvaloniaXKCD.Converters;

public class MathAddConverter<T> : IValueConverter where T : INumber<T>
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is null)
            return default(T);
        if (parameter is null)
            return value;

        T x = Cast(value);
        T y = Cast(parameter);
        return x + y;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is null)
            return default(T);
        if (parameter is null)
            return value;

        T x = Cast(value);
        T y = Cast(parameter);
        return x - y;
    }

    private static T Cast(object obj)
    {
        return obj switch
        {
            T t => t,
            IConvertible c => T.CreateChecked(c.ToDouble(CultureInfo.InvariantCulture)),
            _ => throw new InvalidCastException($"Cannot convert {obj.GetType()} to {typeof(T)}")
        };
    }
}
