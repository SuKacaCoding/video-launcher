using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class DirectlyEnterDirectoryStrategy : IEnterPathStrategy
{
    public static DirectlyEnterDirectoryStrategy Instance => LazyInstance.Value;
    private static readonly Lazy<DirectlyEnterDirectoryStrategy> LazyInstance = new();

    /// <exception cref="ArgumentException"></exception>
    public Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (enterPath.Path is null)
            throw new ArgumentException(message: ".Path mustn't be null.", paramName: nameof(enterPath));
        return Task.FromResult(Path.GetFullPath(enterPath.Path));
    }
}