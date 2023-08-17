using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class GoBackToRootPathStrategy : IEnterPathStrategy
{
    public static GoBackToRootPathStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<GoBackToRootPathStrategy> LazyInstance = new();

    public Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        string pathRoot = Path.GetPathRoot(Path.GetFullPath(enterPath.Path))!;
        lastEnteredPathByDrive[pathRoot] = pathRoot;
        return Task.FromResult(pathRoot);
    }
}