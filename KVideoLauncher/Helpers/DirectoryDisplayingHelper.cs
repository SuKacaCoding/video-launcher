﻿using KVideoLauncher.Data;
using KVideoLauncher.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Helpers;

public static class DirectoryDisplayingHelper
{
    /// <remarks>
    /// Yield returns <see langword="null"/> as a separator between parent and child infos.
    /// </remarks>
    public static async IAsyncEnumerable<DirectoryDisplayingInfo?> EnumerateInfosAsync()
    {
        await foreach (var info in EnumerateHierarchicalParentInfosAsync())
            yield return info;
        yield return null;
        await foreach (var info in EnumerateIndentedChildrenInfosAsync())
            yield return info;
    }

    /// <exception cref="ArgumentException"></exception>
    public static async Task SetCurrentDirectoryAsync(string directoryPath)
    {
        if (!await directoryPath.DirectoryExistsAsync())
        {
            throw new ArgumentException
                (message: "must be an existing directory path.", paramName: nameof(directoryPath));
        }

        s_currentDirectory = new DirectoryInfo(directoryPath);
        s_depthIsUpToDate = false;
    }

    private static async IAsyncEnumerable<DirectoryDisplayingInfo> EnumerateHierarchicalParentInfosAsync()
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
                (displayName: $"{new string(c: ' ', s_depth)}{parentDirectory.Name}", parentDirectory.FullName);
            s_depth += 2;
        }

        s_depthIsUpToDate = true;
    }

    /// <remarks>
    /// Make sure <see cref="EnumerateHierarchicalParentInfosAsync"/> is called before if the
    /// current directory is changed.
    /// </remarks>
    private static async IAsyncEnumerable<DirectoryDisplayingInfo> EnumerateIndentedChildrenInfosAsync()
    {
        Debug.Assert(condition: s_currentDirectory is { }, message: "s_currentDirectory is { }");
        Debug.Assert(s_depthIsUpToDate, message: "s_depthIsUpToDate");

        using IEnumerator<DirectoryInfo> subdirectoriesEnumerator = await Task.Run
            (() => s_currentDirectory.EnumerateDirectories().GetEnumerator());
        while (await Task.Run(() => subdirectoriesEnumerator.MoveNext()))
        {
            var subdirectory = subdirectoriesEnumerator.Current;

            if (subdirectory.IsCommon())
            {
                yield return new DirectoryDisplayingInfo
                    (displayName: $"{new string(c: ' ', s_depth)}{subdirectory.Name}", subdirectory.FullName);
            }
        }
    }

    private static DirectoryInfo? s_currentDirectory;
    private static int s_depth;
    private static bool s_depthIsUpToDate;
}