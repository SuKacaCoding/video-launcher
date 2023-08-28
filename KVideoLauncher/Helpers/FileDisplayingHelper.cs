using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        {
            throw new ArgumentException
                (message: "must be an existing directory path.", paramName: nameof(directoryPath));
        }

        var directoryInfo = new DirectoryInfo(directoryPath);
        FileInfo[] files = await FileHelper.GetCommonFilesAsync(directoryInfo);

        IEnumerable<FileInfo> videos = files.GetVideoFiles(videoFileExtensions);
        IEnumerable<FileInfo> subtitles = files.GetSubtitleFiles(subtitleFileExtensions);
        subtitles = subtitles as FileInfo[] ?? subtitles.ToArray();

        foreach (var video in videos)
        {
            yield return video.HasExternalSubtitle(subtitles)
                ? new FileDisplayingInfo(video.Name, video.FullName, FileDisplayingType.VideoWithSubtitle)
                : new FileDisplayingInfo(video.Name, video.FullName, FileDisplayingType.Video);
        }
    }
}