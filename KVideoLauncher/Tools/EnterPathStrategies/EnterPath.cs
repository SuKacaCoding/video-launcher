using System;
using System.Collections.Generic;

namespace KVideoLauncher.Tools.EnterPathStrategies;

public class EnterPath
{
    public IEnterPathStrategy? Strategy { set; private get; }
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();
    public Dictionary<string, string> LastEnteredPathByDrive { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string? Enter() => Path is null ? null : Strategy?.Enter(this);
}