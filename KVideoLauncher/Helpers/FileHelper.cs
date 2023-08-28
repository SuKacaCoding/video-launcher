using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KVideoLauncher.Extensions;

namespace KVideoLauncher.Helpers;

public static class FileHelper
{
    public static Task<FileInfo[]> GetCommonFilesAsync(DirectoryInfo directoryInfo)
    {
        return Task.Run
        (
            () =>
            {
                IEnumerable<FileInfo> commonFiles = directoryInfo.EnumerateFiles().Where(info => info.IsCommon());
                return commonFiles as FileInfo[] ?? commonFiles.ToArray();
            }
        );
    }
}