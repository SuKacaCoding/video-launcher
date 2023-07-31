using System.Globalization;
using System.Windows.Data;
using KVideoLauncher.Data.Enums;

namespace KVideoLauncher.Tools.Converters;

public class FileDisplayingTypeToIconFontStringConverter : IValueConverter
{
    public object Convert
    (
        object? value, Type targetType, object? parameter,
        CultureInfo culture
    )
    {
        if (value is not FileDisplayingType displayingType)
            return string.Empty;

        return displayingType switch
        {
            FileDisplayingType.Video => "\ue600",
            FileDisplayingType.VideoWithSubtitle => "\ue668",
            FileDisplayingType.Subtitle => "\ue69e",
            _ => throw new ArgumentOutOfRangeException(nameof(displayingType))
        };
    }

    public object ConvertBack
    (
        object? value, Type targetType, object? parameter,
        CultureInfo culture
    ) =>
        throw new NotImplementedException();
}