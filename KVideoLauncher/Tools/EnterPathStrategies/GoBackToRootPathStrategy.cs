using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Data;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class GoBackToRootPathStrategy : IEnterPathStrategy
{
    public static GoBackToRootPathStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<GoBackToRootPathStrategy> LazyInstance = new();

    public async Task<string> Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        string pathRoot = Path.GetPathRoot(Path.GetFullPath(enterPath.Path))!;
        (await SettingsInfo.GetInstanceAsync()).LastEnteredPathByDrive[pathRoot] = pathRoot;
        return pathRoot;
    }
}