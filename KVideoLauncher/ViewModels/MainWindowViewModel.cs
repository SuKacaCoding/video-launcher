using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using KVideoLauncher.Models;

namespace KVideoLauncher.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<DriveInfo> Drives { get; } = new(DataFileHelper.DriveInfos);
}