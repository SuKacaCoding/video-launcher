﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using KVideoLauncher.Data;
using KVideoLauncher.Extensions;
using KVideoLauncher.Helpers;
using KVideoLauncher.Models;
using KVideoLauncher.Properties.Lang;
using KVideoLauncher.Tools.Strategies.EnterPath;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region CanExecutes

    private bool CanSwitchToNextPlaylist => CurrentPlaylistIndex < _historicalPlaylists.Count - 1;
    private bool CanSwitchToPreviousPlaylist => CurrentPlaylistIndex >= 1;

    #endregion CanExecutes

    #region Binding properties

    [ObservableProperty] private int _listDirectorySelectedIndex = -1;
    [ObservableProperty] private int _listFilesSelectedIndex = -1;
    [ObservableProperty] private int _listPlaylistSelectedIndex = -1;
    [ObservableProperty] private Visibility _windowVisibility = Visibility.Visible;

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
    private const int StoredPlaylistsMaxCount = 30;

    private static readonly string SettingsFilePath = Path.Join
        (Utils.SettingsDirectoryPath, path2: "Settings.json");

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
        _historicalPlaylists[CurrentPlaylistIndex].Clear();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        await SaveSettingsAsync();
        await ExecuteExitCommand();

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
    private async Task InitializeDataAsync()
    {
        await _settings;

        RefreshDrives();
        await Task.WhenAll
        (
            InitializePinnedDirectoriesAsync(),
            ExecuteStartupCommand()
        );
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
        UpdateCurrentPlaylist();
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
    private async Task PlayAsync()
    {
        var settings = await _settings;
        if (!settings.PlayCommands.Any())
            return;

        var fileDisplayingInfos = new List<FileDisplayingInfo>();
        bool fromPlaylist = false;
        if (Playlist.Count > 0)
        {
            fileDisplayingInfos.AddRange(Playlist);
            fromPlaylist = true;
        }
        else if (Files.Count == 1)
            fileDisplayingInfos.Add(Files[0]);
        else if (ListFilesSelectedIndex != -1)
            fileDisplayingInfos.Add(Files[ListFilesSelectedIndex]);
        else
        {
            Growl.InfoGlobal(Labels.NothingToPlay);
            return;
        }

        try
        {
            WindowVisibility = Visibility.Hidden;

            if (fromPlaylist)
            {
                SaveCurrentPlaylist();
                CreatePlaylist();
                Playlist.Clear();
            }

            await SaveSettingsAsync();

            await VideoPlayerHelper.LaunchAndWaitAsync
                (settings.PlayCommands, filePaths: fileDisplayingInfos.Select(info => info.File));

            WindowVisibility = Visibility.Visible;
        }
        catch (Win32Exception ex)
        {
            WindowVisibility = Visibility.Visible;

            ExceptionDisplayingHelper.Display
            (
                $"""
                 {Labels.MakeSurePlayCommandIsCorrect}

                 {ex.Message}
                 """
            );
        }
    }

    [RelayCommand]
    private async Task RefreshCurrentPlaylistAsync()
    {
        List<FileDisplayingInfo> currentPlaylist = _historicalPlaylists[CurrentPlaylistIndex];
        currentPlaylist.Clear();

        int newListPlaylistSelectedIndex = ListPlaylistSelectedIndex;
        for (int i = 0; i < Playlist.Count; i++)
        {
            var fileDisplayingInfo = Playlist[i];
            var newFileDisplayingInfo = await fileDisplayingInfo.Refresh((await _settings).SubtitleFileExtensions);
            if (newFileDisplayingInfo is { })
                currentPlaylist.Add(newFileDisplayingInfo);
            else if (i == newListPlaylistSelectedIndex)
                newListPlaylistSelectedIndex = -1;
        }

        UpdateCurrentPlaylist();
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;
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
        _historicalPlaylists[CurrentPlaylistIndex].RemoveAt(currentListPlaylistSelectedIndex);
        UpdateCurrentPlaylist();
        ListPlaylistSelectedIndex = currentListPlaylistSelectedIndex >= Playlist.Count
            ? -1
            : currentListPlaylistSelectedIndex;
    }

    [RelayCommand]
    private void SwitchToLatestPlaylist()
    {
        RemoveCurrentPlaylistIfNecessary();

        CurrentPlaylistIndex = _historicalPlaylists.Count - 1;
        UpdateCurrentPlaylist();
    }

    [RelayCommand(CanExecute = nameof(CanSwitchToNextPlaylist))]
    private void SwitchToNextPlaylist()
    {
        bool removed = RemoveCurrentPlaylistIfNecessary();
        if (removed)
            SwitchToNextPlaylistCommand.NotifyCanExecuteChanged();
        else
            CurrentPlaylistIndex++;
        UpdateCurrentPlaylist();
    }

    [RelayCommand(CanExecute = nameof(CanSwitchToPreviousPlaylist))]
    private void SwitchToPreviousPlaylist()
    {
        RemoveCurrentPlaylistIfNecessary();

        CurrentPlaylistIndex--;
        UpdateCurrentPlaylist();
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

        UpdateCurrentPlaylist();
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;
    }

    private void CommonMoveListPlaylistSelection(int offset)
    {
        int newListPlaylistSelectedIndex = ListPlaylistSelectedIndex + offset;
        if (ListPlaylistSelectedIndex == -1 ||
            newListPlaylistSelectedIndex < 0 ||
            newListPlaylistSelectedIndex >= Playlist.Count)
            return;

        List<FileDisplayingInfo> currentPlaylist = _historicalPlaylists[CurrentPlaylistIndex];
        (currentPlaylist[ListPlaylistSelectedIndex], currentPlaylist[newListPlaylistSelectedIndex]) =
            (currentPlaylist[newListPlaylistSelectedIndex], currentPlaylist[ListPlaylistSelectedIndex]);

        UpdateCurrentPlaylist();
        ListPlaylistSelectedIndex = newListPlaylistSelectedIndex;
    }

    private void CreatePlaylist()
    {
        _historicalPlaylists.Add(new List<FileDisplayingInfo>());
        CurrentPlaylistIndex = _historicalPlaylists.Count - 1;
    }

    private void DirectoryNotExistsCallback()
    {
        RefreshDrives();

        Growl.InfoGlobal(Labels.TargetDirectoryDoesNotExist);
    }

    private async Task ExecuteExitCommand()
    {
        var settings = await _settings;
        string? command = settings.ExitCommand;
        if (command is null)
            return;

        var processStartInfo = new ProcessStartInfo(command);
        try
        {
            var process = Process.Start(processStartInfo);
            if (process is { })
                await process.WaitForExitAsync();
        }
        catch (Win32Exception ex)
        {
            ExceptionDisplayingHelper.Display(ex);
        }
    }

    private async Task ExecuteStartupCommand()
    {
        var settings = await _settings;
        string? command = settings.StartupCommand;
        if (command is null)
            return;

        var processStartInfo = new ProcessStartInfo(command);
        try
        {
            var process = Process.Start(processStartInfo);
            if (process is { })
            {
                await process.WaitForExitAsync();
                Growl.InfoGlobal(Labels.StartupCommandSuccessfullyExecuted);
            }
        }
        catch (Win32Exception ex)
        {
            ExceptionDisplayingHelper.Display(ex);
        }
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

    private async Task InitializePinnedDirectoriesAsync()
    {
        var settings = await _settings;

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
        CreatePlaylist();
    }

    private bool InsertIntoCurrentPlaylistCore(int index, FileDisplayingInfo info)
    {
        if (_historicalPlaylists[CurrentPlaylistIndex].Count == PlaylistFilesMaxCount)
        {
            Growl.InfoGlobal(Labels.PlaylistFilesCountHasReachedLimit);
            return false;
        }

        _historicalPlaylists[CurrentPlaylistIndex].Insert(index, info);
        return true;
    }

    private bool RemoveCurrentPlaylistIfNecessary()
    {
        if (CurrentPlaylistIndex == _historicalPlaylists.Count - 1 || Playlist.Count != 0)
            return false;
        _historicalPlaylists.RemoveAt(CurrentPlaylistIndex);
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

    private void SaveCurrentPlaylist()
    {
        int lastIndexOfHistoricalPlaylists = _historicalPlaylists.Count - 1;
        if (CurrentPlaylistIndex != lastIndexOfHistoricalPlaylists)
        {
            _historicalPlaylists.RemoveAt(lastIndexOfHistoricalPlaylists);
            _historicalPlaylists.RemoveAt(CurrentPlaylistIndex);
            _historicalPlaylists.Add(Playlist.ToList());
        }

        if (_historicalPlaylists.Count > StoredPlaylistsMaxCount)
            _historicalPlaylists.RemoveAt(0);
    }

    private async Task SaveSettingsAsync()
    {
        if (!_settings.IsStarted)
            return;

        await RemoveRedundantPairsInPathEntryFrequenciesAsync();
        await SetNewHistoricalPlaylistsForSettingsAsync();

        try
        {
            Directory.CreateDirectory(Utils.SettingsDirectoryPath);

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
        var settings = await _settings;
        settings.HistoricalPlaylists.Clear();
        settings.HistoricalPlaylists.AddRange(_historicalPlaylists.Take(_historicalPlaylists.Count - 1));
    }

    private void UpdateCurrentPlaylist()
    {
        Playlist.Clear();
        Playlist.AddRange(_historicalPlaylists[CurrentPlaylistIndex]);
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

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SwitchToPreviousPlaylistCommand))]
    [NotifyCanExecuteChangedFor(nameof(SwitchToNextPlaylistCommand))]
    private int _currentPlaylistIndex;

    #endregion Private fields
}