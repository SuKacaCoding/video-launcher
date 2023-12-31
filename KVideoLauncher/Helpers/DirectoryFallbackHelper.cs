﻿using KVideoLauncher.Extensions;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Helpers;

public static class DirectoryFallbackHelper
{
    /// <exception cref="ArgumentException"></exception>
    public static async Task<string?> FallbackAsync(string directoryPath)
    {
        if (await directoryPath.DirectoryExistsAsync())
        {
            throw new ArgumentException
                (message: "mustn't be an existing directory path.", paramName: nameof(directoryPath));
        }

        while (!directoryPath.IsRootPath())
        {
            directoryPath = Path.Join(directoryPath, path2: "..");
            if (await directoryPath.DirectoryExistsAsync())
                return directoryPath;
        }

        return null;
    }
}