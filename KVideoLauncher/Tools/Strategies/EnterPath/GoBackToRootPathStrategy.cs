﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.Strategies.EnterPath;

public class GoBackToRootPathStrategy : IEnterPathStrategy
{
    public static GoBackToRootPathStrategy Instance => LazyInstance.Value;

    /// <exception cref="ArgumentException"></exception>
    public Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (enterPath.Path is null)
            throw new ArgumentException(message: ".Path mustn't be null.", paramName: nameof(enterPath));
        string pathRoot = Path.GetPathRoot(Path.GetFullPath(enterPath.Path))!;
        lastEnteredPathByDrive[pathRoot] = pathRoot;
        return Task.FromResult(pathRoot);
    }

    private static readonly Lazy<GoBackToRootPathStrategy> LazyInstance = new();
}