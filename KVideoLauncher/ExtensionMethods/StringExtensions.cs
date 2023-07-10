using System.IO;

namespace KVideoLauncher.ExtensionMethods;

public static class StringExtensions
{
    public static DriveInfo? GetDriveInfo(this string driveStr)
    {
        try
        {
            var ret = new DriveInfo(driveStr);
            return ret;
        }
        catch
        {
            return null;
        }
    }
}