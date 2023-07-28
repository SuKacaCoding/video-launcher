using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Data;
using KVideoLauncher.Data.Enums;
using KVideoLauncher.Extensions;

namespace KVideoLauncher.Helpers;

public static class FileDisplayingHelper
{
    public static async IAsyncEnumerable<FileDisplayingInfo> EnumerateFilesInDirectoryAsync(string directoryPath)
    {
        Debug.Assert(condition: Directory.Exists(directoryPath), message: "Directory.Exists(directoryPath)");

        var directoryInfo = new DirectoryInfo(directoryPath);
        using IEnumerator<FileInfo> filesInDirectoryEnumerator =
            await Task.Run(() => directoryInfo.EnumerateFiles().GetEnumerator());
        while (await Task.Run(() => filesInDirectoryEnumerator.MoveNext()))
        {
            var currentFileInfo = filesInDirectoryEnumerator.Current;
            if (!currentFileInfo.IsCommon())
                continue;

            string correspondingSubtitleName = $"{Path.GetFileNameWithoutExtension(currentFileInfo.Name)}.srt";
            string correspondingSubtitlePath = Path.Join(directoryPath, correspondingSubtitleName);
            var fileDisplayingType = await Task.Run(() => File.Exists(correspondingSubtitlePath))
                ? FileDisplayingType.VideoWithSubtitle
                : FileDisplayingType.Video;

            yield return new FileDisplayingInfo(currentFileInfo.Name, currentFileInfo, fileDisplayingType);
        }
    }
}