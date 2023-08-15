using System.Collections.Generic;
using System.IO;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class GoBackToRootPathStrategy : IEnterPathStrategy
{
    public static GoBackToRootPathStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<GoBackToRootPathStrategy> LazyInstance = new();

    public string Enter(EnterPath enterPath, Dictionary<string, string> lastEnteredPathByDrive)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        string pathRoot = Path.GetPathRoot(Path.GetFullPath(enterPath.Path))!;
        lastEnteredPathByDrive[pathRoot] = pathRoot;
        return pathRoot;
    }
}