using System.IO;

namespace KVideoLauncher.Extensions;

public static class FileSystemInfoExtensions
{
    public static bool IsCommon(this FileSystemInfo info) =>
        !info.Attributes.HasFlag(FileAttributes.System) &&
        !info.Attributes.HasFlag(FileAttributes.Hidden);
}