using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Extensions;

namespace KVideoLauncher.Helpers;

public static class DirectoryFallbackHelper
{
    public static async Task<string?> FallbackAsync(string directoryPath)
    {
        Debug.Assert
            (condition: !await directoryPath.DirectoryExistsAsync(), message: "!await directoryPath.DirectoryExists()");

        while (!Path.IsPathRooted(directoryPath))
        {
            directoryPath = Path.Join(directoryPath, path2: "..");
            if (await directoryPath.DirectoryExistsAsync())
                return directoryPath;
        }

        return null;
    }
}