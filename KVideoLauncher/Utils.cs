using System.IO;

namespace KVideoLauncher;

public static class Utils
{
    public static readonly string SettingsDirectoryPath = Path.Join
    (
        path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        path2: "KVideoLauncher"
    );
}