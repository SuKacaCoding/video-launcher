using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KVideoLauncher.Models;

public class SettingsModel
{
    public Dictionary<string, string> LastEnteredPathByDrive { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> VideoFileExtensions { get; set; } =
        new[] { "avi", "flv", "mkv", "mov", "mp4", "rm", "rmvb", "ts", "ps", "wmv" }.AsReadOnly();

    public IEnumerable<string> SubtitleFileExtensions { get; set; } =
        new[] { "srt", "sub", "idx", "ass" }.AsReadOnly();

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
    }

    /// <exception cref="UnauthorizedAccessException" />
    /// <exception cref="IOException" />
    public static async Task SaveInstanceAsync()
    {
        Directory.CreateDirectory(SettingsDirectoryPath);

        await using var createStream = File.Create(SettingsFilePath);
        await JsonSerializer.SerializeAsync
            (createStream, s_instance, options: new JsonSerializerOptions { WriteIndented = true });
    }
}