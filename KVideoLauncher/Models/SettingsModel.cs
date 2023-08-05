using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
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

    private static readonly string SettingsDirectoryPath = Path.Join
    (
        path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        path2: "KVideoLauncher"
    );

    private static readonly string SettingsFilePath = Path.Join
        (SettingsDirectoryPath, path2: "Settings.json");

    private static SettingsModel? s_instance;

    public static async Task<SettingsModel> GetInstanceAsync()
    {
        if (s_instance is { })
            return s_instance;
        if (!Path.Exists(SettingsFilePath))
        {
            s_instance = new SettingsModel();
            return s_instance;
        }

        try
        {
            await using var openStream = File.OpenRead(SettingsFilePath);
            var settingsModel = await JsonSerializer.DeserializeAsync<SettingsModel>
                (openStream, options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            s_instance = settingsModel ?? new SettingsModel();

            return s_instance;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            s_instance = new SettingsModel();
            return s_instance;
        }
        // TODO: Catch JsonException.
    }

    /// <exception cref="UnauthorizedAccessException" />
    /// <exception cref="IOException" />
    public static async Task SaveInstanceAsync()
    {
        if (s_instance is null)
            return;

        Directory.CreateDirectory(SettingsDirectoryPath);

        await using var createStream = File.Create(SettingsFilePath);
        await JsonSerializer.SerializeAsync
        (
            createStream, s_instance,
            options: new JsonSerializerOptions
                { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
        );
    }
}