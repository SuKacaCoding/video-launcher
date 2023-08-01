namespace KVideoLauncher.Data;

public class DirectoryDisplayingInfo
{
    public string DisplayName { get; }
    public string Directory { get; }

    public DirectoryDisplayingInfo(string displayName, string directory)
    {
        DisplayName = displayName;
        Directory = directory;
    }
}