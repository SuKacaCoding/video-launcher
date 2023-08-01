using KVideoLauncher.Data.Enums;

namespace KVideoLauncher.Data;

public class FileDisplayingInfo
{
    public string DisplayName { get; }
    public string File { get; }
    public FileDisplayingType DisplayingType { get; }

    public FileDisplayingInfo(string displayName, string file, FileDisplayingType displayingType)
    {
        DisplayName = displayName;
        File = file;
        DisplayingType = displayingType;
    }
}