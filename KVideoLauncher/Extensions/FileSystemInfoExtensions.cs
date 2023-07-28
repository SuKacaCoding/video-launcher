using System.IO;
using KVideoLauncher.Helpers;

namespace KVideoLauncher.Extensions;

public static class FileSystemInfoExtensions
{
    public static bool IsCommon(this FileSystemInfo info) => FileAttributesHelper.IsCommon(info);
}