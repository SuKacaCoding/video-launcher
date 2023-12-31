﻿using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace KVideoLauncher.Helpers;

public static class VideoPlayerHelper
{
    /// <exception cref="System.ComponentModel.Win32Exception"/>
    public static async Task LaunchAndWaitAsync(IEnumerable<string> commands, IEnumerable<string> filePaths)
    {
        var stringBuilder = new StringBuilder();
        foreach (string filePath in filePaths)
            stringBuilder.AppendLine(filePath);

        try
        {
            Directory.CreateDirectory(Utils.SettingsDirectoryPath);
            await File.WriteAllTextAsync(PlaylistFilePath, contents: stringBuilder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SecurityException)
        {
            ExceptionDisplayingHelper.Display(ex);
            return;
        }

        foreach (string command in commands)
        {
            var processStartInfo = new ProcessStartInfo
                (command.Replace(TextToReplace, newValue: $"\"{PlaylistFilePath}\""));
            var process = Process.Start(processStartInfo);
            if (process is { })
                await process.WaitForExitAsync();
        }
    }

    private const string TextToReplace = "<playlist.m3u8>";
    private static readonly string PlaylistFilePath = Path.Join(Utils.SettingsDirectoryPath, path2: "Playlist.m3u8");
}