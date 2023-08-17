using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Extensions;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDriveStrategy : IEnterPathStrategy
{
    public static EnterDriveStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<EnterDriveStrategy> LazyInstance = new();

    public async Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        Debug.Assert
        (
            condition: Directory.GetParent(enterPath.Path) is null,
            message: "Directory.GetParent(enterPath.Path) is null"
        );


        string normalizedDrivePath = Path.GetFullPath(enterPath.Path);
        bool driveHasLastEnteredPath = lastEnteredPathByDrive.TryGetValue
            (normalizedDrivePath, value: out string? ret);

        if (driveHasLastEnteredPath && await ret.DirectoryExistsAsync())
            return ret!;

        lastEnteredPathByDrive[normalizedDrivePath] = normalizedDrivePath;
        return normalizedDrivePath;
    }
}