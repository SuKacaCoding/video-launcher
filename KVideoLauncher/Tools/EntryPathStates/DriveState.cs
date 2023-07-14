using System;
using System.Diagnostics;
using System.IO;

namespace KVideoLauncher.Tools.EntryPathStates;

public class DriveState : IEntryPathState
{
    public string Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        if (Directory.GetParent(enterPath.Path) is { })
            throw new NotImplementedException();

        string normalizedDrivePath = Path.GetFullPath(enterPath.Path);
        bool driveHasLastEnteredPath = enterPath.LastEnteredPathByDrive.TryGetValue
            (normalizedDrivePath, value: out string? ret);

        if (driveHasLastEnteredPath)
            return ret!;

        enterPath.LastEnteredPathByDrive[normalizedDrivePath] = normalizedDrivePath;
        return normalizedDrivePath;
    }
}