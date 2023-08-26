using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using KVideoLauncher.Data;
using KVideoLauncher.Extensions;
using KVideoLauncher.Helpers;
using KVideoLauncher.Models;
using KVideoLauncher.Properties.Lang;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using KVideoLauncher.Tools.Strategies.EnterPath;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Binding properties

    [ObservableProperty] private int _listDirectorySelectedIndex;

    [ObservableProperty] private int _listFilesSelectedIndex;
    [ObservableProperty] private int _listPlaylistSelectedIndex = -1;

    #endregion Binding properties

    #region ObservableCollections

    public ObservableCollection<DirectoryDisplayingInfo> Directories { get; } = new();
    public ObservableCollection<DriveInfo> Drives { get; } = new();
    public ObservableCollection<FileDisplayingInfo> Files { get; } = new();
    public ObservableCollection<FrequentDirectoryDisplayingInfo> FrequentDirectories { get; } = new();
    public ObservableCollection<FileDisplayingInfo> Playlist { get; } = new();

    #endregion ObservableCollections

    #region Constants

    private const int FrequentlyEnteredDirectoriesMaxCount = 7;
    private const int PinnedDirectoriesMaxCount = 5;
    private const int PlaylistFilesMaxCount = 72;
    private const int StoredFrequentlyEnteredDirectoriesMaxCount = 30;

    private static readonly string SettingsDirectoryPath = Path.Join
    (
        path1: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        path2: "KVideoLauncher"
    );

    private static readonly string SettingsFilePath = Path.Join
        (SettingsDirectoryPath, path2: "Settings.json");

    #endregion Constants

    #region RelayCommanmds

    [RelayCommand]
    private async Task ChangeDirectoryAsync(DirectoryDisplayingInfo? info)
    {
        if (info is null)
            return;

        await CommonChangeDirectoryAsync(info.Directory, EnterDirectoryStrategy.Instance);
        await UpdateFrequentDirectoriesAsync();
    }

    [RelayCommand]
    private Task ChangeDriveAsync(string? driveName) => CommonChangeDirectoryAsync
        (driveName, EnterDriveStrategy.Instance);

    [RelayCommand]
    private void ClearCurrentPlaylist()
    {
        Playlist.Clear();
        _historicalPlaylists[_currentPlaylistIndex].Clear();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        await SaveSettingsAsync();

        Application.Current.Shutdown();
    }

    [RelayCommand]
    private Task GoBackToRootDirectoryAsync() => CommonChangeDirectoryAsync
        (EnterPath.Instance.Path, GoBackToRootPathStrategy.Instance);

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
    private void InsertAllFilesIntoCurrentPlaylist()
    {
        CommonInsertRangeIntoCurrentPlaylist(Files);
    }

    [RelayCommand]
    private void InsertIntoCurrentPlaylist(FileDisplayingInfo? info)
    {
        if (info is null)
            return;

        int newListPlaylistSelectedIndex = ListPlaylistSelectedIndex + 1;
        InsertIntoCurrentPlaylistCore(newListPlaylistSelectedIndex, info);
        Playlist.Clear();
        Playlist.AddRange(_historicalPlaylists[_currentPlaylistIndex]);
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;

        ListFilesSelectedIndex = (ListFilesSelectedIndex + 1).MathMod(Files.Count);
    }

    [RelayCommand]
    private void InsertSelectedFileAndAboveIntoCurrentPlaylist()
    {
        CommonInsertRangeIntoCurrentPlaylist(Files.Take(ListFilesSelectedIndex + 1));
    }

    [RelayCommand]
    private void InsertSelectedFileAndBelowIntoCurrentPlaylist()
    {
        CommonInsertRangeIntoCurrentPlaylist(Files.TakeLast(Files.Count - ListFilesSelectedIndex));
    }

    [RelayCommand]
    private void MoveListPlaylistSelectionDown()
    {
        CommonMoveListPlaylistSelection(1);
    }

    [RelayCommand]
    private void MoveListPlaylistSelectionUp()
    {
        CommonMoveListPlaylistSelection(-1);
    }

    [RelayCommand]
    private Task RefreshDirectoryAsync() => CommonChangeDirectoryAsync
        (EnterPath.Instance.Path, DirectlyEnterDirectoryStrategy.Instance);

    [RelayCommand]
    private void RefreshDrives()
    {
        Drives.Clear();
        Drives.AddRange(DriveInfo.GetDrives().Where(info => info.IsReady));
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

    #endregion RelayCommanmds

    #region Private methods

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

    private void CommonInsertRangeIntoCurrentPlaylist(IEnumerable<FileDisplayingInfo> infos)
    {
        bool canContinue = true;
        int newListPlaylistSelectedIndex = ListPlaylistSelectedIndex;
        foreach (var info in infos)
        {
            if (!canContinue)
                break;

            newListPlaylistSelectedIndex++;
            canContinue = InsertIntoCurrentPlaylistCore(newListPlaylistSelectedIndex, info);
        }

        Playlist.Clear();
        Playlist.AddRange(_historicalPlaylists[_currentPlaylistIndex]);
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;
    }

    private void CommonMoveListPlaylistSelection(int offset)
    {
        int newListPlaylistSelectedIndex = ListPlaylistSelectedIndex + offset;
        if (ListPlaylistSelectedIndex == -1 ||
            newListPlaylistSelectedIndex < 0 ||
            newListPlaylistSelectedIndex >= Playlist.Count)
            return;

        List<FileDisplayingInfo> currentPlaylist = _historicalPlaylists[_currentPlaylistIndex];
        (currentPlaylist[ListPlaylistSelectedIndex], currentPlaylist[newListPlaylistSelectedIndex]) =
            (currentPlaylist[newListPlaylistSelectedIndex], currentPlaylist[ListPlaylistSelectedIndex]);

        Playlist.Clear();
        Playlist.AddRange(currentPlaylist);
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;
    }

    private void DirectoryNotExistsCallback()
    {
        RefreshDrives();

        Growl.InfoGlobal(Labels.TargetDirectoryDoesNotExist);
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

    private bool InsertIntoCurrentPlaylistCore(int index, FileDisplayingInfo info)
    {
        if (_historicalPlaylists[_currentPlaylistIndex].Count == PlaylistFilesMaxCount)
        {
            Growl.InfoGlobal(Labels.PlaylistFilesCountHasReachedLimit);
            return false;
        }

        _historicalPlaylists[_currentPlaylistIndex].Insert(index, info);
        return true;
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

    private async Task SetNewHistoricalPlaylistsForSettingsAsync()
    {
        int latestPlaylistIndex = _historicalPlaylists.Count - 1;
        if (_historicalPlaylists[latestPlaylistIndex].Count == 0)
            _historicalPlaylists.RemoveAt(latestPlaylistIndex);

        var settings = await _settings;
        settings.HistoricalPlaylists.Clear();
        settings.HistoricalPlaylists.AddRange(_historicalPlaylists);
    }

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

    #endregion Private methods

    #region Private fields

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
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                ExceptionDisplayingHelper.Display(ex);
                return new SettingsModel();
            }
        }
    );

    private int _currentPlaylistIndex;

    #endregion Private fields
}