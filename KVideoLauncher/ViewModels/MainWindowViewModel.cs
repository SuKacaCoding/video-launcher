using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools.Extension;
using KVideoLauncher.Data;
using KVideoLauncher.Helpers;
using KVideoLauncher.Tools.EnterPathStrategies;
using MessageBox = HandyControl.Controls.MessageBox;

namespace KVideoLauncher.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<DriveInfo> Drives { get; } = new();
    public ObservableCollection<DirectoryDisplayingInfo> Directories { get; } = new();
    public string? SelectedDriveName { get; set; }

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
    private void ChangeDrive()
    {
        ChangeDirectory(SelectedDriveName, EnterDriveStrategy.Instance, RefreshDrives);
    }

    [RelayCommand]
    private void RefreshDirectory()
    {
        // TODO: Use RefreshDirectoryStrategy instead. 
        ChangeDirectory(EnterPath.Instance.Path, EnterDriveStrategy.Instance, RefreshDrives);
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
            MessageBox.Show(e.Message, caption: "Exception", icon: MessageBoxImage.Error);
        }
    }
}