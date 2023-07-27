using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Helpers;

public static class SettingsFileReadWriteHelper
{
    private static readonly string AppSettingsPath = Path.Join
        (path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), path2: "KVideoLauncher");

    public static async Task<string?> ReadAsync(string fileName)
    {
        string fullPath = Path.Join(AppSettingsPath, fileName);
        if (!Path.Exists(fullPath))
            return null;

        try
        {
            string ret = await File.ReadAllTextAsync(fullPath);
            return ret;
        }
        catch
        {
            return null;
        }
    }

    public static async Task WriteAsync(string fileName, string content)
    {
        Directory.CreateDirectory(AppSettingsPath);
        string fullPath = Path.Join(AppSettingsPath, fileName);

        try
        {
            await File.WriteAllTextAsync(fullPath, content);
        }
        catch (Exception ex)
        {
            ExceptionDisplayHelper.Display(ex);
        }
    }
}