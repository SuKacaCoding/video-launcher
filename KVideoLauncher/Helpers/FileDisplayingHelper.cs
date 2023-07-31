using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KVideoLauncher.Data;
using KVideoLauncher.Data.Enums;
using KVideoLauncher.Extensions;
using KVideoLauncher.Models;

namespace KVideoLauncher.Helpers;

public static class FileDisplayingHelper
{
    public static async IAsyncEnumerable<FileDisplayingInfo> EnumerateVideosInDirectoryAsync(string directoryPath)
    {
        Debug.Assert(condition: Directory.Exists(directoryPath), message: "Directory.Exists(directoryPath)");

        var directoryInfo = new DirectoryInfo(directoryPath);
        IEnumerable<FileInfo> files = await Task.Run
        (
            () =>
            {
                IEnumerable<FileInfo> commonFiles = directoryInfo.EnumerateFiles().Where(info => info.IsCommon());
                return commonFiles as FileInfo[] ?? commonFiles.ToArray();
            }
        );
        var settings = await SettingsModel.GetInstanceAsync();

        IEnumerable<FileInfo> videos = files.Where
        (
            info => settings.VideoFileExtensions.Any
                (extension => string.Equals(extension, info.Extension, StringComparison.OrdinalIgnoreCase))
        );
        IEnumerable<FileInfo> subtitles = files.Where
        (
            info => settings.SubtitleFileExtensions.Any
                (extension => string.Equals(extension, info.Extension, StringComparison.OrdinalIgnoreCase))
        );
        subtitles = subtitles as FileInfo[] ?? subtitles.ToArray();

        foreach (var video in videos)
        {
            if (subtitles.Any
                (
                    subtitle => video.Name.StartsWith
                        (value: Path.GetFileNameWithoutExtension(subtitle.Name), StringComparison.OrdinalIgnoreCase)
                ))
                yield return new FileDisplayingInfo(video.Name, video, FileDisplayingType.VideoWithSubtitle);
            else
                yield return new FileDisplayingInfo(video.Name, video, FileDisplayingType.Video);
        }
    }
}