using System.IO;
using KVideoLauncher.Data.Enums;

namespace KVideoLauncher.Data;

public class FileDisplayingInfo
{
    public string DisplayName { get; }
    public FileInfo File { get; }
    public FileDisplayingType DisplayingType { get; }

    public FileDisplayingInfo(string displayName, FileInfo file, FileDisplayingType displayingType)
    {
        DisplayName = displayName;
        File = file;
        DisplayingType = displayingType;
    }
}