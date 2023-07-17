using System;
using System.Diagnostics;
using System.IO;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class RefreshDirectoryStrategy : IEnterPathStrategy
{
    public static RefreshDirectoryStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<RefreshDirectoryStrategy> LazyInstance = new();

    public string Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        return Path.GetFullPath(enterPath.Path);
    }
}