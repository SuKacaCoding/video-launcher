using System.Collections.Generic;
using System.IO;
using System.Linq;
using KVideoLauncher.Data;

namespace KVideoLauncher.Helpers;

public static class DirectoryChildrenHelper
{
    public static IEnumerable<DirectoryDisplayingInfo> GetHierarchicalDirectoryDisplayingInfos
        (string directoryPath)
    {
        var currentDirectory = new DirectoryInfo(directoryPath);

        DirectoryInfo[] subdirectories = currentDirectory.GetDirectories();

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

        return ret;
    }
}