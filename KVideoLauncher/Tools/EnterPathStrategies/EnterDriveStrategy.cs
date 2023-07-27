using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Data;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDriveStrategy : IEnterPathStrategy
{
    public static EnterDriveStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<EnterDriveStrategy> LazyInstance = new();

    public async Task<string> Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        Debug.Assert
        (
            condition: Directory.GetParent(enterPath.Path) is null,
            message: "Directory.GetParent(enterPath.Path) is null"
        );

        var settingsInfo = await SettingsInfo.GetInstanceAsync();

        string normalizedDrivePath = Path.GetFullPath(enterPath.Path);
        bool driveHasLastEnteredPath = settingsInfo.LastEnteredPathByDrive.TryGetValue
            (normalizedDrivePath, value: out string? ret);

        if (driveHasLastEnteredPath)
            return ret!;

        settingsInfo.LastEnteredPathByDrive[normalizedDrivePath] = normalizedDrivePath;
        return normalizedDrivePath;
    }
}