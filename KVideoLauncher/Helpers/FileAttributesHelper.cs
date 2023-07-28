using System.IO;

namespace KVideoLauncher.Helpers;

public static class FileAttributesHelper
{
    public static bool IsCommon(FileSystemInfo info) =>
        !info.Attributes.HasFlag(FileAttributes.System) &&
        !info.Attributes.HasFlag(FileAttributes.Hidden);
}