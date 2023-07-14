using System.Diagnostics;
using System.IO;

namespace KVideoLauncher.Helpers;

public static class DirectoryFallbackHelper
{
    public static string? Fallback(string directoryPath)
    {
        Debug.Assert(condition: !Directory.Exists(directoryPath), message: "!Directory.Exists(directoryPath)");

        while (!Path.IsPathRooted(directoryPath))
        {
            directoryPath = Path.Combine(directoryPath, path2: "..");
            if (Directory.Exists(directoryPath))
                return directoryPath;
        }

        return null;
    }
}