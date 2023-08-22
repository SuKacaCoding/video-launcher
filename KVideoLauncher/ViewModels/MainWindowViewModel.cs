﻿using System.Collections.Generic;
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
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using KVideoLauncher.Data;
using KVideoLauncher.Extensions;
using KVideoLauncher.Helpers;
using KVideoLauncher.Models;
using KVideoLauncher.Properties.Lang;
using KVideoLauncher.Tools.EnterPathStrategies;
using Nito.AsyncEx;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Binding properties

    [ObservableProperty] private int _listDirectorySelectedIndex;

    [ObservableProperty] private int _listPlaylistSelectedIndex = -1;

    [ObservableProperty] private int _listFilesSelectedIndex;

    #endregion

    #region ObservableCollections

    public ObservableCollection<DriveInfo> Drives { get; } = new();
    public ObservableCollection<DirectoryDisplayingInfo> Directories { get; } = new();
    public ObservableCollection<FileDisplayingInfo> Files { get; } = new();
    public ObservableCollection<FrequentDirectoryDisplayingInfo> FrequentDirectories { get; } = new();
    public ObservableCollection<FileDisplayingInfo> Playlist { get; } = new();

    #endregion

    #region Constants

    private const int PinnedDirectoriesMaxCount = 5;
    private const int FrequentlyEnteredDirectoriesMaxCount = 7;
    private const int StoredFrequentlyEnteredDirectoriesMaxCount = 30;
    private const int HistoricalPlaylistsMaxCount = 30;
    private const int PlaylistFilesMaxCount = 50;

    private static readonly string SettingsDirectoryPath = Path.Join
    (
        path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        path2: "KVideoLauncher"
    );

    private static readonly string SettingsFilePath = Path.Join
        (SettingsDirectoryPath, path2: "Settings.json");

    #endregion

    #region RelayCommanmds

    [RelayCommand]
    private async Task InitializeData()
    {
        var settings = await _settings;

        RefreshDrives();

        _pinnedDirectories.AddRange
        (
            (await settings.PinnedDirectories
                .WhereAsync(info => info.Directory.DirectoryExistsAsync()))
            .Take(PinnedDirectoriesMaxCount)
            .Select(info => new FrequentDirectoryDisplayingInfo(info.DisplayName, info.Directory, isPinned: true))
        );
        await UpdateFrequentDirectoriesAsync();

        foreach (IEnumerable<FileDisplayingInfo> playlist in settings.HistoricalPlaylists)
            _historicalPlaylists.Add(playlist.ToList());
        _historicalPlaylists.Add(new List<FileDisplayingInfo>());
        _currentPlaylistIndex = _historicalPlaylists.Count - 1;
    }

    [RelayCommand]
    private void RefreshDrives()
    {
        Drives.Clear();
        Drives.AddRange(DriveInfo.GetDrives().Where(info => info.IsReady));
    }

    [RelayCommand]
    private Task ChangeDriveAsync(string? driveName) => CommonChangeDirectoryAsync
        (driveName, EnterDriveStrategy.Instance);

    [RelayCommand]
    private Task RefreshDirectoryAsync() => CommonChangeDirectoryAsync
        (EnterPath.Instance.Path, DirectlyEnterDirectoryStrategy.Instance);

    [RelayCommand]
    private async Task ChangeDirectoryAsync(DirectoryDisplayingInfo? info)
    {
        if (info is null)
            return;

        await CommonChangeDirectoryAsync(info.Directory, EnterDirectoryStrategy.Instance);
        await UpdateFrequentDirectoriesAsync();
    }

    [RelayCommand]
    private Task GoBackToRootDirectoryAsync() => CommonChangeDirectoryAsync
        (EnterPath.Instance.Path, GoBackToRootPathStrategy.Instance);

    [RelayCommand]
    private void InsertIntoCurrentPlaylist(FileDisplayingInfo? info)
    {
        if (info is null)
            return;
        if (_historicalPlaylists[_currentPlaylistIndex].Count == PlaylistFilesMaxCount)
        {
            Growl.InfoGlobal(Labels.PlaylistFilesCountHasReachedLimit);
            return;
        }

        int newListPlaylistSelectedIndex = ListPlaylistSelectedIndex + 1;
        _historicalPlaylists[_currentPlaylistIndex].Insert(newListPlaylistSelectedIndex, info);
        Playlist.Clear();
        Playlist.AddRange(_historicalPlaylists[_currentPlaylistIndex]);
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;

        ListFilesSelectedIndex = (ListFilesSelectedIndex + 1).MathMod(Files.Count);
    }

    [RelayCommand]
    private void RemovePlaylistSelectedFile()
    {
        if (ListPlaylistSelectedIndex == -1)
            return;

        int currentListPlaylistSelectedIndex = ListPlaylistSelectedIndex;
        _historicalPlaylists[_currentPlaylistIndex].RemoveAt(currentListPlaylistSelectedIndex);
        Playlist.Clear();
        Playlist.AddRange(_historicalPlaylists[_currentPlaylistIndex]);
        ListPlaylistSelectedIndex = currentListPlaylistSelectedIndex >= Playlist.Count
            ? -1
            : currentListPlaylistSelectedIndex;
    }

    [RelayCommand]
    private async Task GoToParentDirectoryAsync()
    {
        if (ListPlaylistSelectedIndex == -1)
            return;

        var info = Playlist[ListPlaylistSelectedIndex];
        await CommonChangeDirectoryAsync
            (directoryPath: Path.GetDirectoryName(info.File), EnterDirectoryStrategy.Instance);
        await UpdateFrequentDirectoriesAsync();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        await SaveSettingsAsync();

        Application.Current.Shutdown();
    }

    #endregion

    #region Private methods

    private async Task UpdateFrequentDirectoriesAsync()
    {
        string? currentDirectory = EnterPath.Instance.Path;

        if (currentDirectory is { })
        {
            var settings = await _settings;
            settings.EntryFrequencyByPath[currentDirectory] =
                (settings.EntryFrequencyByPath.TryGetValue
                    (currentDirectory, value: out int frequency)
                    ? frequency
                    : 0) + 1;
        }

        PriorityQueue<string, int> priorityQueue = await GetMaxPriorityQueueFromPathEntryFrequenciesAsync();

        _frequentlyEnteredDirectories.Clear();
        while (priorityQueue.Count > 0 &&
               _frequentlyEnteredDirectories.Count < FrequentlyEnteredDirectoriesMaxCount)
        {
            string directoryPath = priorityQueue.Dequeue();
            var directoryInfo = new DirectoryInfo(directoryPath);
            _frequentlyEnteredDirectories.Add
            (
                new FrequentDirectoryDisplayingInfo
                (
                    directoryInfo.Name, directoryPath, isPinned: false
                )
            );
        }

        FrequentDirectories.Clear();
        FrequentDirectories.AddRange(_pinnedDirectories);
        FrequentDirectories.AddRange(_frequentlyEnteredDirectories);
    }

    private async Task<PriorityQueue<string, int>> GetMaxPriorityQueueFromPathEntryFrequenciesAsync()
    {
        var settings = await _settings;

        var priorityQueue = new PriorityQueue<string, int>(Comparer<int>.Create((a, b) => b - a));
        foreach (KeyValuePair<string, int> pathFrequencyPair in settings.EntryFrequencyByPath)
        {
            if (await pathFrequencyPair.Key.DirectoryExistsAsync())
                priorityQueue.Enqueue(pathFrequencyPair.Key, pathFrequencyPair.Value);
        }

        return priorityQueue;
    }

    private async Task CommonChangeDirectoryAsync
        (string? directoryPath, IEnterPathStrategy strategy)
    {
        if (directoryPath is null)
            return;
        if (!await directoryPath.DirectoryExistsAsync())
        {
            directoryPath = await DirectoryFallbackHelper.FallbackAsync(directoryPath)
                            ?? (await EnterPath.Instance.Path.DirectoryExistsAsync()
                                ? EnterPath.Instance.Path
                                : null);
            DirectoryNotExistsCallback();
            await ChangeDirectoryCoreAsync(directoryPath, DirectlyEnterDirectoryStrategy.Instance);
            return;
        }

        await ChangeDirectoryCoreAsync(directoryPath, strategy);
    }

    private async Task ChangeDirectoryCoreAsync(string? directoryPath, IEnterPathStrategy strategy)
    {
        try
        {
            EnterPath.Instance.Path = directoryPath;
            EnterPath.Instance.Strategy = strategy;

            Directories.Clear();
            Files.Clear();

            string? outputPath = await EnterPath.Instance.Enter((await _settings).LastEnteredPathByDrive);
            if (outputPath is null)
                return;

            await DirectoryDisplayingHelper.SetCurrentDirectoryAsync(outputPath);

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

            await foreach (var displayingInfo in FileDisplayingHelper.EnumerateVideosInDirectoryAsync
                           (
                               outputPath,
                               (await _settings).VideoFileExtensions,
                               (await _settings).SubtitleFileExtensions
                           ))
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

        Growl.InfoGlobal(Labels.TargetDirectoryDoesNotExist);
    }

    private async Task RemoveRedundantPairsInPathEntryFrequenciesAsync()
    {
        var settings = await _settings;
        var newEntryFrequencyByPath = new Dictionary<string, int>();
        IDictionary<string, int> oldEntryFrequencyByPath = settings.EntryFrequencyByPath;
        PriorityQueue<string, int> priorityQueue = await GetMaxPriorityQueueFromPathEntryFrequenciesAsync();
        for (int i = 0; i < StoredFrequentlyEnteredDirectoriesMaxCount; i++)
        {
            if (priorityQueue.Count == 0)
                break;

            string directoryPath = priorityQueue.Dequeue();
            newEntryFrequencyByPath[directoryPath] = oldEntryFrequencyByPath[directoryPath];
        }

        settings.EntryFrequencyByPath.Clear();
        settings.EntryFrequencyByPath.AddRange(newEntryFrequencyByPath);
    }

    private async Task SetNewHistoricalPlaylistsForSettingsAsync()
    {
        int latestPlaylistIndex = _historicalPlaylists.Count - 1;
        if (_historicalPlaylists[latestPlaylistIndex].Count == 0)
            _historicalPlaylists.RemoveAt(latestPlaylistIndex);

        var settings = await _settings;
        settings.HistoricalPlaylists.Clear();
        settings.HistoricalPlaylists.AddRange(_historicalPlaylists);
    }

    private async Task SaveSettingsAsync()
    {
        if (!_settings.IsStarted)
            return;

        await RemoveRedundantPairsInPathEntryFrequenciesAsync();
        await SetNewHistoricalPlaylistsForSettingsAsync();

        try
        {
            Directory.CreateDirectory(SettingsDirectoryPath);

            await using var createStream = File.Create(SettingsFilePath);
            await JsonSerializer.SerializeAsync
            (
                createStream, value: await _settings,
                options: new JsonSerializerOptions
                    { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }
            );
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            ExceptionDisplayingHelper.Display(ex);
        }
    }

    #endregion

    #region Private fields

    private int _currentPlaylistIndex;

    private readonly List<FrequentDirectoryDisplayingInfo> _frequentlyEnteredDirectories =
        new(FrequentlyEnteredDirectoriesMaxCount);

    private readonly List<List<FileDisplayingInfo>> _historicalPlaylists = new();

    private readonly List<FrequentDirectoryDisplayingInfo> _pinnedDirectories = new(PinnedDirectoriesMaxCount);

    private readonly AsyncLazy<SettingsModel> _settings = new
    (
        async () =>
        {
            if (!await SettingsFilePath.FileExistsAsync())
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

    #endregion
}