using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using KVideoLauncher.Extensions;
using KVideoLauncher.Helpers;

namespace KVideoLauncher.Data;

public class SettingsInfo
{
    public Dictionary<string, string> LastEnteredPathByDrive { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private const string SettingsFileName = "settings.json";

    private static SettingsInfo? s_instance;
    private static readonly object SyncRoot = new();

    public static async Task<SettingsInfo> GetInstanceAsync()
    {
        if (s_instance is { })
            return s_instance;

        string? settingsContent = await SettingsFileReadWriteHelper.ReadAsync(SettingsFileName);
        if (settingsContent is null)
            s_instance = new SettingsInfo();
        else
        {
            try
            {
                s_instance = JsonSerializer.Deserialize<SettingsInfo>(settingsContent) ?? new SettingsInfo();
            }
            catch
            {
                s_instance = new SettingsInfo();
            }
        }

        return s_instance;
    }

    public static async Task SaveInstanceAsync()
    {
        if (s_instance is null)
            return;

        string settingsContent = JsonSerializer.Serialize
            (s_instance, options: new JsonSerializerOptions { WriteIndented = true });
        await settingsContent.WriteToSettingsFileAsync(SettingsFileName);
    }
}