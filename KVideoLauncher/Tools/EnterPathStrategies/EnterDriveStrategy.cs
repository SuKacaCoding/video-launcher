using System;
using System.Diagnostics;
using System.IO;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDriveStrategy : IEnterPathStrategy
{
    public static EnterDriveStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<EnterDriveStrategy> LazyInstance = new();

    public string Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        Debug.Assert
        (
            condition: Directory.GetParent(enterPath.Path) is null,
            message: "Directory.GetParent(enterPath.Path) is null"
        );

        string normalizedDrivePath = Path.GetFullPath(enterPath.Path);
        bool driveHasLastEnteredPath = enterPath.LastEnteredPathByDrive.TryGetValue
            (normalizedDrivePath, value: out string? ret);

        if (driveHasLastEnteredPath)
            return ret!;

        enterPath.LastEnteredPathByDrive[normalizedDrivePath] = normalizedDrivePath;
        return normalizedDrivePath;
    }
}