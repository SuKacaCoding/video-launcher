﻿using System.Collections.Generic;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterPath
{
    public IEnterPathStrategy? Strategy { set; private get; }
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();

    public string Enter(Dictionary<string, string> lastEnteredPathByDrive)
    {
        Debug.Assert(condition: Path is { }, message: "Path is { }");
        Debug.Assert(condition: Strategy is { }, message: "Strategy is { }");
        Path = Strategy.EnterAsync(enterPath: this, lastEnteredPathByDrive);
        return Path;
    }
}