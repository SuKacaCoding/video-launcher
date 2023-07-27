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
using KVideoLauncher.Tools.EnterPathStrategies;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<DriveInfo> Drives { get; } = new();
    public ObservableCollection<DirectoryDisplayingInfo> Directories { get; } = new();

    public MainWindowViewModel()
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
    private Task ChangeDrive(string? driveName) => CommonChangeDirectory
        (driveName, EnterDriveStrategy.Instance);

    [RelayCommand]
    private Task RefreshDirectory() => CommonChangeDirectory
        (EnterPath.Instance.Path, RefreshDirectoryStrategy.Instance);

    [RelayCommand]
    private Task ChangeDirectory
        (object? parameter) => parameter is DirectoryDisplayingInfo info
        ? CommonChangeDirectory(info.Directory.FullName, EnterDirectoryStrategy.Instance)
        : Task.CompletedTask;

    [RelayCommand]
    private Task GoBackToRootDirectory() => CommonChangeDirectory
        (EnterPath.Instance.Path, GoBackToRootPathStrategy.Instance);

    private async Task CommonChangeDirectory
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

            DirectoryDisplayingHelper.SetCurrentDirectory(outputPath);

            IAsyncEnumerable<DirectoryDisplayingInfo> parentInfos =
                DirectoryDisplayingHelper.GetHierarchicalParentInfos();
            int parentLevelCount = 0;
            await foreach (var displayingInfo in parentInfos)
            {
                Directories.Add(displayingInfo);
                parentLevelCount++;
            }

            ListDirectorySelectedIndex = parentLevelCount;

            await foreach (var displayingInfo in DirectoryDisplayingHelper.GetIndentedChildrenInfos())
                Directories.Add(displayingInfo);
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException)
        {
            ExceptionDisplayHelper.Display(e);
        }
    }

    private void DirectoryNotExistsCallback()
    {
        RefreshDrives();
    }

    [RelayCommand]
    private async Task Exit()
    {
        await SettingsInfo.SaveInstanceAsync();
        Application.Current.Shutdown();
    }

    [ObservableProperty] private int _listDirectorySelectedIndex;
}