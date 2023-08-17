using System.Collections.Generic;
using System.Threading.Tasks;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterPath
{
    public IEnterPathStrategy? Strategy { set; private get; }
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();

    public async Task<string?> Enter(IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (Path is null)
            return null;
        Debug.Assert(condition: Strategy is { }, message: "Strategy is { }");
        Path = await Strategy.Enter(enterPath: this, lastEnteredPathByDrive);
        return Path;
    }
}