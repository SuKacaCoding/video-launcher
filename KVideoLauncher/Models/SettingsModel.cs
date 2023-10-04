﻿using KVideoLauncher.Data;
using KVideoLauncher.Tools.Converters;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace KVideoLauncher.Models;

public class SettingsModel
{
    #region Setting properties generated by KVideoLauncher

    [JsonConverter(typeof(CaseInsensitiveDictionaryJsonConverter<int>))]
    public IDictionary<string, int> EntryFrequencyByPath { get; init; } =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public ICollection<IEnumerable<FileDisplayingInfo>> HistoricalPlaylists { get; init; } =
        new Collection<IEnumerable<FileDisplayingInfo>>();

    [JsonConverter(typeof(CaseInsensitiveDictionaryJsonConverter<string>))]
    public IDictionary<string, string> LastEnteredPathByDrive { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    #endregion Setting properties generated by KVideoLauncher

    #region Setting properties for users to customize

    public string? StartupCommand { get; init; }

    public string? ExitCommand { get; init; }

    public IEnumerable<DirectoryDisplayingInfo> PinnedDirectories { get; init; } =
        Enumerable.Empty<DirectoryDisplayingInfo>();

    public IEnumerable<string> PlayCommands { get; init; } = Enumerable.Empty<string>();

    public IEnumerable<string> SubtitleFileExtensions { get; init; } =
        new[] { ".SRT", ".SUB", ".IDX", ".ASS" }.AsReadOnly();

    public IEnumerable<string> VideoFileExtensions { get; init; } =
        new[] { ".AVI", ".FLV", ".MKV", ".MOV", ".MP4", ".RM", ".RMVB", ".TS", ".PS", ".WMV" }.AsReadOnly();

    #endregion Setting properties for users to customize
}