using System.IO;
using System.Threading.Tasks;

namespace KVideoLauncher.Extensions;

public static class StringExtensions
{
    public static Task<bool> DirectoryExistsAsync(this string? path) => Task.Run(() => Directory.Exists(path));

    public static Task<bool> FileExistsAsync(this string? path) => Task.Run(() => File.Exists(path));
}