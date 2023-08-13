using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using KVideoLauncher.Data;
using KVideoLauncher.Helpers;
using KVideoLauncher.Models;
using KVideoLauncher.Tools.EnterPathStrategies;
using Nito.AsyncEx;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<DriveInfo> Drives { get; } = new();
    public ObservableCollection<DirectoryDisplayingInfo> Directories { get; } = new();
    public ObservableCollection<FileDisplayingInfo> Files { get; } = new();
    public ObservableCollection<FrequentDirectoryDisplayingInfo> FrequentDirectories { get; } = new();

    private const int PinnedDirectoriesMaxCount = 7;

    private static readonly string SettingsDirectoryPath = Path.Join
    (
        path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        path2: "KVideoLauncher"
    );

    private static readonly string SettingsFilePath = Path.Join
        (SettingsDirectoryPath, path2: "Settings.json");

    [RelayCommand]
    private async Task InitializeData()
    {
        var settings = await SettingsModel.GetInstanceAsync();

        RefreshDrives();

        _pinnedDirectories.AddRange
        (
            settings.PinnedDirectories
                .Where(info => Directory.Exists(info.Directory))
                .Take(PinnedDirectoriesMaxCount)
                .Select(info => new FrequentDirectoryDisplayingInfo(info.DisplayName, info.Directory, isPinned: true))
        );
        FrequentDirectories.AddRange(_pinnedDirectories);
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        Drives.Clear();
        Drives.AddRange(DriveInfo.GetDrives());
    }

    [RelayCommand]
    private Task ChangeDriveAsync(string? driveName) => CommonChangeDirectoryAsync
        (driveName, EnterDriveStrategy.Instance);

    [RelayCommand]
    private Task RefreshDirectoryAsync() => CommonChangeDirectoryAsync
        (EnterPath.Instance.Path, RefreshDirectoryStrategy.Instance);

    [RelayCommand]
    private Task ChangeDirectoryAsync
        (object? parameter) => parameter is DirectoryDisplayingInfo info
        ? CommonChangeDirectoryAsync(info.Directory, EnterDirectoryStrategy.Instance)
        : Task.CompletedTask;

    [RelayCommand]
    private Task GoBackToRootDirectoryAsync() => CommonChangeDirectoryAsync
        (EnterPath.Instance.Path, GoBackToRootPathStrategy.Instance);

    private async Task CommonChangeDirectoryAsync
        (string? directoryPath, IEnterPathStrategy strategy)
    {
        if (directoryPath is null)
            return;
        if (!Directory.Exists(directoryPath))
        {
            // TODO: Show a notification when fallback.
            directoryPath = DirectoryFallbackHelper.Fallback(directoryPath)
                            ?? (Directory.Exists(EnterPath.Instance.Path)
                                ? EnterPath.Instance.Path
                                : null);
            DirectoryNotExistsCallback();
        }

        await ChangeDirectoryCoreAsync(directoryPath, strategy);
    }

    private async Task ChangeDirectoryCoreAsync(string? directoryPath, IEnterPathStrategy strategy)
    {
        try
        {
            EnterPath.Instance.Path = directoryPath;
            EnterPath.Instance.Strategy = strategy;

            string outputPath = await EnterPath.Instance.Enter();

            Directories.Clear();
            Files.Clear();

            DirectoryDisplayingHelper.SetCurrentDirectory(outputPath);

            int parentLevelCount = 0;
            int firstChildDirectoryIndex = 0;
            await foreach (var displayingInfo in DirectoryDisplayingHelper.EnumerateInfosAsync())
            {
                if (displayingInfo is null)
                {
                    firstChildDirectoryIndex = parentLevelCount;
                    continue;
                }

                Directories.Add(displayingInfo);
                parentLevelCount++;
            }

            ListDirectorySelectedIndex = firstChildDirectoryIndex;

            await foreach (var displayingInfo in FileDisplayingHelper.EnumerateVideosInDirectoryAsync(outputPath))
                Files.Add(displayingInfo);
        }
        catch (Exception ex) when (ex is SecurityException or UnauthorizedAccessException)
        {
            ExceptionDisplayingHelper.Display(ex);
        }
    }

    private void DirectoryNotExistsCallback()
    {
        RefreshDrives();
    }

    [RelayCommand]
    private static async Task ExitAsync()
    {
        await SettingsModel.SaveInstanceAsync();

        Application.Current.Shutdown();
    }

    public async Task SaveSettingsAsync()
    {
        if (!_settings.IsStarted)
            return;

        try
        {
            Directory.CreateDirectory(SettingsDirectoryPath);

            await using var createStream = File.Create(SettingsFilePath);
            await JsonSerializer.SerializeAsync
            (
                createStream, _settings,
                options: new JsonSerializerOptions
                    { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            );
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            ExceptionDisplayingHelper.Display(ex);
        }
    }

    private readonly List<FrequentDirectoryDisplayingInfo> _pinnedDirectories = new(PinnedDirectoriesMaxCount);

    private readonly AsyncLazy<SettingsModel> _settings = new
    (
        async () =>
        {
            if (!Path.Exists(SettingsFilePath))
                return new SettingsModel();

            try
            {
                await using var openStream = File.OpenRead(SettingsFilePath);
                var settingsModel = await JsonSerializer.DeserializeAsync<SettingsModel>
                    (openStream, options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return settingsModel ?? new SettingsModel();
            }
            catch (Exception ex)when (ex is UnauthorizedAccessException or IOException)
            {
                ExceptionDisplayingHelper.Display(ex);
                return new SettingsModel();
            }
        }
    );

    [ObservableProperty] private int _listDirectorySelectedIndex;
}