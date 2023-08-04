namespace KVideoLauncher.Data;

public class FrequentDirectoryDisplayingInfo : DirectoryDisplayingInfo
{
    public bool IsPinned { get; }

    public FrequentDirectoryDisplayingInfo(string displayName, string directory, bool isPinned)
        : base(displayName, directory) => IsPinned = isPinned;
}