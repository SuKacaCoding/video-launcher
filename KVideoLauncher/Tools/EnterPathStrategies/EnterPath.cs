using System.Collections.Generic;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterPath
{
    public IEnterPathStrategy? Strategy { set; private get; }
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();

    public string? Enter(IDictionary<string, string> lastEnteredPathByDrive)
    {
        if (Path is null)
            return null;
        Debug.Assert(condition: Strategy is { }, message: "Strategy is { }");
        Path = Strategy.Enter(enterPath: this, lastEnteredPathByDrive);
        return Path;
    }
}