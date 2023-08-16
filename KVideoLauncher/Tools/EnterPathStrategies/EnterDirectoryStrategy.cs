﻿using System.Collections.Generic;
using System.IO;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDirectoryStrategy : IEnterPathStrategy
{
    public static EnterDirectoryStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<EnterDirectoryStrategy> LazyInstance = new();

    public string Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        string normalizedPath = Path.GetFullPath(enterPath.Path);

        lastEnteredPathByDrive[Directory.GetDirectoryRoot(normalizedPath)] = normalizedPath;

        return normalizedPath;
    }
}