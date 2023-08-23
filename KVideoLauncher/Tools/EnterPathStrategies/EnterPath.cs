using System.Collections.Generic;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterPath
{
    public IEnterPathStrategy? Strategy { set; private get; }
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();
    
    /// <exception cref="NullReferenceException"></exception>
    public async Task<string?> Enter(IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (Path is null)
            return null;
        if (Strategy is null)
            throw new NullReferenceException($"{nameof(Strategy)} mustn't be null.");

        Path = await Strategy.Enter(enterPath: this, lastEnteredPathByDrive);
        return Path;
    }
}