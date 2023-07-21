using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
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
    private void ChangeDrive(string? driveName)
    {
        ChangeDirectory(driveName, EnterDriveStrategy.Instance, RefreshDrives);
    }

    [RelayCommand]
    private void RefreshDirectory()
    {
        ChangeDirectory(EnterPath.Instance.Path, RefreshDirectoryStrategy.Instance, RefreshDrives);
    }

    private void ChangeDirectory
        (string? directoryPath, IEnterPathStrategy strategy, Action directoryNotExistsCallback)
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
            directoryNotExistsCallback();
        }

        try
        {
            EnterPath.Instance.Path = directoryPath;
            EnterPath.Instance.Strategy = strategy;

            string? outputPath = EnterPath.Instance.Enter();

            Directories.Clear();
            if (outputPath is null)
                return;

            IEnumerable<DirectoryDisplayingInfo> displayingInfos =
                DirectoryChildrenHelper.GetHierarchicalDirectoryDisplayingInfos(outputPath);
            Directories.AddRange(displayingInfos);
        }
        catch (Exception e) when (e is SecurityException or UnauthorizedAccessException)
        {
            ExceptionDisplayHelper.Display(e);
        }
    }
}