using System;
using System.Diagnostics;
using System.IO;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDirectoryStrategy : IEnterPathStrategy
{
    public static EnterDirectoryStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<EnterDirectoryStrategy> LazyInstance = new();

    public string Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        return Path.GetFullPath(enterPath.Path);
    }
}