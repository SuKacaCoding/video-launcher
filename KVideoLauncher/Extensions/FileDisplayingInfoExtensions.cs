using KVideoLauncher.Data;
using KVideoLauncher.Data.Enums;
using KVideoLauncher.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Extensions;

public static class FileDisplayingInfoExtensions
{
    public static async Task<FileDisplayingInfo?> Refresh
        (this FileDisplayingInfo fileDisplayingInfo, IEnumerable<string> subtitleFileExtensions)
    {
        if (!File.Exists(fileDisplayingInfo.File))
            return null;

        var videoFile = new FileInfo(fileDisplayingInfo.File);
        FileInfo[] filesInSameDirectory = await FileHelper.GetCommonFilesAsync(videoFile.Directory!);
        IEnumerable<FileInfo> subtitles = filesInSameDirectory.GetSubtitleFiles(subtitleFileExtensions);
        return videoFile.HasExternalSubtitle(subtitles)
            ? new FileDisplayingInfo
                (fileDisplayingInfo.DisplayName, fileDisplayingInfo.File, FileDisplayingType.VideoWithSubtitle)
            : new FileDisplayingInfo(fileDisplayingInfo.DisplayName, fileDisplayingInfo.File, FileDisplayingType.Video);
    }
}