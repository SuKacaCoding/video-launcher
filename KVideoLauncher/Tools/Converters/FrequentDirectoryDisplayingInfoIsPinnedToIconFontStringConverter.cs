using System.Globalization;
using System.Windows.Data;

namespace KVideoLauncher.Tools.Converters;

public class FrequentDirectoryDisplayingInfoIsPinnedToIconFontStringConverter : IValueConverter
{
    public object Convert
    (
        object? value, Type targetType, object? parameter,
        CultureInfo culture
    ) =>
        value is true ? "\ue7a4" : string.Empty;

    public object ConvertBack
    (
        object? value, Type targetType, object? parameter,
        CultureInfo culture
    ) =>
        throw new NotImplementedException();
}