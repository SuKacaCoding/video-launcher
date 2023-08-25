namespace KVideoLauncher.Data;

public class DirectoryDisplayingInfo
{
    public string Directory { get; }
    public string DisplayName { get; }

    public DirectoryDisplayingInfo(string displayName, string directory)
    {
        DisplayName = displayName;
        Directory = directory;
    }
}