using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class RefreshDirectoryStrategy : IEnterPathStrategy
{
    public static RefreshDirectoryStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<RefreshDirectoryStrategy> LazyInstance = new();

    public Task<string> Enter(EnterPath enterPath)
    {
        Debug.Assert(condition: enterPath.Path != null, message: "enterPath.Path != null");
        return Task.FromResult(Path.GetFullPath(enterPath.Path));
    }
}