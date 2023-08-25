using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterDirectoryStrategy : IEnterPathStrategy
{
    public static EnterDirectoryStrategy Instance => LazyInstance.Value;

    /// <exception cref="ArgumentException"></exception>
    public Task<string> Enter(EnterPath enterPath, IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (enterPath.Path is null)
            throw new ArgumentException(message: ".Path mustn't be null.", paramName: nameof(enterPath));
        string normalizedPath = Path.GetFullPath(enterPath.Path);

        lastEnteredPathByDrive[Directory.GetDirectoryRoot(normalizedPath)] = normalizedPath;

        return Task.FromResult(normalizedPath);
    }

    private static readonly Lazy<EnterDirectoryStrategy> LazyInstance = new();
}