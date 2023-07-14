using System.IO;

namespace KVideoLauncher.Data;

public class DirectoryDisplayingInfo
{
    public string DisplayName { get; }
    public DirectoryInfo Directory { get; }

    public DirectoryDisplayingInfo(string displayName, DirectoryInfo directory)
    {
        DisplayName = displayName;
        Directory = directory;
    }
}