using System.Threading.Tasks;
using KVideoLauncher.Helpers;

namespace KVideoLauncher.Extensions;

public static class StringExtensions
{
    public static Task WriteToSettingsFileAsync(this string content, string fileName) =>
        SettingsFileReadWriteHelper.WriteAsync(fileName, content);
}