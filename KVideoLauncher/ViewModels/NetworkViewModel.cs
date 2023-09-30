using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KVideoLauncher.Tools;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace KVideoLauncher.ViewModels;

public partial class NetworkViewModel : ObservableObject
{
    public ObservableCollection<string> ComputerShares { get; } = new();

    [RelayCommand]
    private async Task RefreshComputerSharesAsync()
    {
        ComputerShares.Clear();

        IsLoadingComputerShares = true;
        LoadingComputerSharesVisibility = Visibility.Visible;

        var networkComputers = new NetworkComputers();
        await foreach (string networkComputer in networkComputers)
        {
            var computerShares = new ComputerShares(networkComputer);
            await foreach (string computerShare in computerShares)
                ComputerShares.Add(computerShare);
        }

        LoadingComputerSharesVisibility = Visibility.Hidden;
        IsLoadingComputerShares = false;
    }

    [ObservableProperty] private bool _isLoadingComputerShares;

    [ObservableProperty] private Visibility _loadingComputerSharesVisibility;
}