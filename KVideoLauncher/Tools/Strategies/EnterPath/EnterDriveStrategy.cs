using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Extensions;

namespace KVideoLauncher.Tools.Strategies.EnterPath;

public class EnterDriveStrategy : IEnterPathStrategy
{
    public static EnterDriveStrategy Instance => LazyInstance.Value;

    /// <exception cref="ArgumentException"></exception>
    public async Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (enterPath.Path is null)
            throw new ArgumentException(message: ".Path mustn't be null.", paramName: nameof(enterPath));
        if (!Path.IsPathRooted(enterPath.Path))
            throw new ArgumentException(message: ".Path must be a rooted path.", paramName: nameof(enterPath));

        string normalizedDrivePath = Path.GetFullPath(enterPath.Path);
        bool driveHasLastEnteredPath = lastEnteredPathByDrive.TryGetValue
            (normalizedDrivePath, value: out string? ret);

        if (driveHasLastEnteredPath && await ret.DirectoryExistsAsync())
            return ret!;

        lastEnteredPathByDrive[normalizedDrivePath] = normalizedDrivePath;
        return normalizedDrivePath;
    }

    private static readonly Lazy<EnterDriveStrategy> LazyInstance = new();
}