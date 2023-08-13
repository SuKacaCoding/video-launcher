using System.Collections.Generic;
using System.Linq;
using KVideoLauncher.Data;

namespace KVideoLauncher.Models;

public class SettingsModel
{
    public Dictionary<string, string> LastEnteredPathByDrive { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> VideoFileExtensions { get; set; } =
        new[] { ".AVI", ".FLV", ".MKV", ".MOV", ".MP4", ".RM", ".RMVB", ".TS", ".PS", ".WMV" }.AsReadOnly();

    public IEnumerable<string> SubtitleFileExtensions { get; set; } =
        new[] { ".SRT", ".SUB", ".IDX", ".ASS" }.AsReadOnly();

    public IEnumerable<DirectoryDisplayingInfo> PinnedDirectories { get; set; } =
        Enumerable.Empty<DirectoryDisplayingInfo>();
}