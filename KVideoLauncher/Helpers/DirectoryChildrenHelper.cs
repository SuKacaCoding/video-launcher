using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KVideoLauncher.Data;

namespace KVideoLauncher.Helpers;

public static class DirectoryChildrenHelper
{
    public static IList<DirectoryDisplayingInfo> GetHierarchicalDirectoryDisplayingInfos
        (string directoryPath)
    {
        Debug.Assert(condition: Directory.Exists(directoryPath), message: "Directory.Exists(directoryPath)");

        var currentDirectory = new DirectoryInfo(directoryPath);

        IEnumerable<DirectoryInfo> subdirectories = currentDirectory.EnumerateDirectories().Where
        (
            info => !(info.Attributes.HasFlag(FileAttributes.System)
                      || info.Attributes.HasFlag(FileAttributes.Hidden))
        );

        var parentDirectories = new List<DirectoryInfo>();
        while (currentDirectory.Parent?.Parent is { })
        {
            currentDirectory = currentDirectory.Parent;
            parentDirectories.Add(currentDirectory);
        }

        parentDirectories.Reverse();

        var ret = new List<DirectoryDisplayingInfo>();
        int depth = 0;

        foreach (var parentDirectory in parentDirectories)
        {
            ret.Add
            (
                new DirectoryDisplayingInfo
                    (displayName: $"{new string(c: ' ', depth)}{parentDirectory.Name}", parentDirectory)
            );
            depth += 2;
        }

        ret.AddRange
        (
            subdirectories.Select
            (
                subdirectory => new DirectoryDisplayingInfo
                    (displayName: $"{new string(c: ' ', depth)}{subdirectory.Name}", subdirectory)
            )
        );

        return ret.AsReadOnly();
    }
}