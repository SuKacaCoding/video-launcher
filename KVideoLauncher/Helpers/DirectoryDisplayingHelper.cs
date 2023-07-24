using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Data;

namespace KVideoLauncher.Helpers;

public static class DirectoryDisplayingHelper
{
    public static async IAsyncEnumerable<DirectoryDisplayingInfo> GetHierarchicalDirectoryDisplayingInfos
        (string directoryPath)
    {
        Debug.Assert(condition: Directory.Exists(directoryPath), message: "Directory.Exists(directoryPath)");

        var currentDirectory = new DirectoryInfo(directoryPath);

        var parentDirectories = new List<DirectoryInfo>();
        var parentDirectoryPointer = currentDirectory;
        await Task.Run
        (
            () =>
            {
                while (parentDirectoryPointer.Parent is { })
                {
                    parentDirectories.Add(parentDirectoryPointer);
                    parentDirectoryPointer = parentDirectoryPointer.Parent;
                }

                parentDirectories.Add(parentDirectoryPointer);
            }
        );
        parentDirectories.Reverse();

        int depth = 0;
        foreach (var parentDirectory in parentDirectories)
        {
            yield return new DirectoryDisplayingInfo
                (displayName: $"{new string(c: ' ', depth)}{parentDirectory.Name}", parentDirectory);
            depth += 2;
        }

        using IEnumerator<DirectoryInfo> subdirectoriesEnumerator = await Task.Run
            (() => currentDirectory.EnumerateDirectories().GetEnumerator());
        while (await Task.Run(() => subdirectoriesEnumerator.MoveNext()))
        {
            var subdirectory = subdirectoriesEnumerator.Current;

            if (!subdirectory.Attributes.HasFlag(FileAttributes.System) &&
                !subdirectory.Attributes.HasFlag(FileAttributes.Hidden))
            {
                yield return new DirectoryDisplayingInfo
                    (displayName: $"{new string(c: ' ', depth)}{subdirectory.Name}", subdirectory);
            }
        }
    }
}