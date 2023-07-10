using System.IO;
using System.Linq;

namespace KVideoLauncher.ExtensionMethods;

public static class StringExtensions
{
    public static DriveInfo? GetDriveInfo(this string driveStr)
    {
        try
        {
            var ret = new DriveInfo(driveStr.ToUpper());
            return DriveInfo.GetDrives().Any(existingDriveInfo => existingDriveInfo.Name == ret.Name) ? ret : null;
        }
        catch
        {
            return null;
        }
    }
}