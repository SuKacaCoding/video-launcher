using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using KVideoLauncher.Data;
using KVideoLauncher.Helpers;
using KVideoLauncher.Models;
using KVideoLauncher.Tools.EnterPathStrategies;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<DriveInfo> Drives { get; } = new();
    public ObservableCollection<DirectoryDisplayingInfo> Directories { get; } = new();
    public ObservableCollection<FileDisplayingInfo> Files { get; } = new();

    [RelayCommand]
    private void InitializeData()
    {
        RefreshDrives();
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
        ? CommonChangeDirectoryAsync(info.Directory.FullName, EnterDirectoryStrategy.Instance)
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

        try
        {
            EnterPath.Instance.Path = directoryPath;
            EnterPath.Instance.Strategy = strategy;

            string outputPath = await EnterPath.Instance.Enter();

            Directories.Clear();
            Files.Clear();

            DirectoryDisplayingHelper.SetCurrentDirectory(outputPath);

            IAsyncEnumerable<DirectoryDisplayingInfo> parentInfos =
                DirectoryDisplayingHelper.EnumerateHierarchicalParentInfosAsync();
            int parentLevelCount = 0;
            await foreach (var displayingInfo in parentInfos)
            {
                Directories.Add(displayingInfo);
                parentLevelCount++;
            }

            ListDirectorySelectedIndex = parentLevelCount;

            await foreach (var displayingInfo in DirectoryDisplayingHelper.EnumerateIndentedChildrenInfosAsync())
                Directories.Add(displayingInfo);

            await foreach (var displayingInfo in FileDisplayingHelper.EnumerateVideosInDirectoryAsync(outputPath))
                Files.Add(displayingInfo);
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException)
        {
            ExceptionDisplayingHelper.Display(e);
        }
    }

    private void DirectoryNotExistsCallback()
    {
        RefreshDrives();
    }

    [RelayCommand]
    private async Task ExitAsync()
    {
        try
        {
            await SettingsModel.SaveInstanceAsync();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            ExceptionDisplayingHelper.Display(ex);
        }

        Application.Current.Shutdown();
    }

    [ObservableProperty] private int _listDirectorySelectedIndex;
}