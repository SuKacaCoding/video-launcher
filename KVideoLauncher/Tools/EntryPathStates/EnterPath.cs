using System;
using System.Collections.Generic;

namespace KVideoLauncher.Tools.EntryPathStates;

public class EnterPath
{
    private IEntryPathState State { set; get; } = new DriveState();
    public string? Path { get; set; }
    public static EnterPath Instance { get; } = new();
    public Dictionary<string, string> LastEnteredPathByDrive { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string? Enter() => Path is null ? null : State.Enter(this);

    public void Reset()
    {
        State = new DriveState();
    }
}