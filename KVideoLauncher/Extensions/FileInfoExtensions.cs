using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KVideoLauncher.Extensions;

public static class FileInfoExtensions
{
    public static bool HasExternalSubtitle(this FileInfo video, IEnumerable<FileInfo> subtitles)
    {
        return subtitles.Any
        (
            subtitle => subtitle.Name.StartsWith
                (value: Path.GetFileNameWithoutExtension(video.Name), StringComparison.OrdinalIgnoreCase)
        );
    }
}