using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KVideoLauncher.Data;
using KVideoLauncher.Data.Enums;
using KVideoLauncher.Extensions;

namespace KVideoLauncher.Helpers;

public static class FileDisplayingHelper
{
    /// <exception cref="ArgumentException"></exception>
    public static async IAsyncEnumerable<FileDisplayingInfo> EnumerateVideosInDirectoryAsync
    (
        string directoryPath,
        IEnumerable<string> videoFileExtensions,
        IEnumerable<string> subtitleFileExtensions
    )
    {
        if (!await directoryPath.DirectoryExistsAsync())
            throw new ArgumentException
                (message: "must be an existing directory path.", paramName: nameof(directoryPath));

        var directoryInfo = new DirectoryInfo(directoryPath);
        IEnumerable<FileInfo> files = await Task.Run
        (
            () =>
            {
                IEnumerable<FileInfo> commonFiles = directoryInfo.EnumerateFiles().Where(info => info.IsGeneral());
                return commonFiles as FileInfo[] ?? commonFiles.ToArray();
            }
        );

        IEnumerable<FileInfo> videos = files.Where
        (
            info => videoFileExtensions.Any
                (extension => string.Equals(extension, info.Extension, StringComparison.OrdinalIgnoreCase))
        );
        IEnumerable<FileInfo> subtitles = files.Where
        (
            info => subtitleFileExtensions.Any
                (extension => string.Equals(extension, info.Extension, StringComparison.OrdinalIgnoreCase))
        );
        subtitles = subtitles as FileInfo[] ?? subtitles.ToArray();

        foreach (var video in videos)
        {
            if (subtitles.Any
                (
                    subtitle => subtitle.Name.StartsWith
                        (value: Path.GetFileNameWithoutExtension(video.Name), StringComparison.OrdinalIgnoreCase)
                ))
                yield return new FileDisplayingInfo(video.Name, video.FullName, FileDisplayingType.VideoWithSubtitle);
            else
                yield return new FileDisplayingInfo(video.Name, video.FullName, FileDisplayingType.Video);
        }
    }
}