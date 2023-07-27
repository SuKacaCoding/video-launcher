using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Data;

namespace KVideoLauncher.Helpers;

public static class DirectoryDisplayingHelper
{
    private static int s_depth;
    private static DirectoryInfo? s_currentDirectory;

    public static void SetCurrentDirectory(string directoryPath)
    {
        Debug.Assert(condition: Directory.Exists(directoryPath), message: "Directory.Exists(directoryPath)");
        s_currentDirectory = new DirectoryInfo(directoryPath);
    }

    public static async IAsyncEnumerable<DirectoryDisplayingInfo> GetHierarchicalParentInfos()
    {
        Debug.Assert(condition: s_currentDirectory is { }, message: "s_currentDirectory is { }");

        var parentDirectories = new List<DirectoryInfo>();
        var parentDirectoryPointer = s_currentDirectory;
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

        s_depth = 0;
        foreach (var parentDirectory in parentDirectories)
        {
            yield return new DirectoryDisplayingInfo
                (displayName: $"{new string(c: ' ', s_depth)}{parentDirectory.Name}", parentDirectory);
            s_depth += 2;
        }
    }

    public static async IAsyncEnumerable<DirectoryDisplayingInfo> GetIndentedChildrenInfos()
    {
        Debug.Assert(condition: s_currentDirectory is { }, message: "s_currentDirectory is { }");

        using IEnumerator<DirectoryInfo> subdirectoriesEnumerator = await Task.Run
            (() => s_currentDirectory.EnumerateDirectories().GetEnumerator());
        while (await Task.Run(() => subdirectoriesEnumerator.MoveNext()))
        {
            var subdirectory = subdirectoriesEnumerator.Current;

            if (!subdirectory.Attributes.HasFlag(FileAttributes.System) &&
                !subdirectory.Attributes.HasFlag(FileAttributes.Hidden))
            {
                yield return new DirectoryDisplayingInfo
                    (displayName: $"{new string(c: ' ', s_depth)}{subdirectory.Name}", subdirectory);
            }
        }
    }
}