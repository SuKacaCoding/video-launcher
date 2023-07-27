using System.IO;
using System.Threading.Tasks;
using KVideoLauncher.Data;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDirectoryStrategy : IEnterPathStrategy
{
    public static EnterDirectoryStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<EnterDirectoryStrategy> LazyInstance = new();

    public async Task<string> Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        string normalizedPath = Path.GetFullPath(enterPath.Path);

        (await SettingsInfo.GetInstanceAsync()).LastEnteredPathByDrive[Directory.GetDirectoryRoot(normalizedPath)] =
            normalizedPath;

        return normalizedPath;
    }
}