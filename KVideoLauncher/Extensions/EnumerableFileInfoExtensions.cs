using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KVideoLauncher.Extensions;

public static class EnumerableFileInfoExtensions
{
    public static IEnumerable<FileInfo> GetSubtitleFiles
    (
        this IEnumerable<FileInfo> files,
        IEnumerable<string> subtitleFileExtensions
    )
    {
        return files.Where
        (
            info => subtitleFileExtensions.Any
                (extension => string.Equals(extension, info.Extension, StringComparison.OrdinalIgnoreCase))
        );
    }

    public static IEnumerable<FileInfo> GetVideoFiles
    (
        this IEnumerable<FileInfo> files,
        IEnumerable<string> videoFileExtensions
    )
    {
        return files.Where
        (
            info => videoFileExtensions.Any
                (extension => string.Equals(extension, info.Extension, StringComparison.OrdinalIgnoreCase))
        );
    }
}