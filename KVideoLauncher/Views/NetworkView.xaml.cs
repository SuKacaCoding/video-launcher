using KVideoLauncher.Extensions;
using KVideoLauncher.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KVideoLauncher.Views;

public partial class NetworkView : UserControl
{
    public NetworkView()
    {
        InitializeComponent();

        _viewModel = (NetworkViewModel)DataContext;
    }

    public async Task PrepareAsync()
    {
        ListComputerShares.FocusOnSelectionOrItself();

        if (_viewModel.RefreshComputerSharesCommand.CanExecute(null) && ListComputerShares.Items.Count == 0)
            await _viewModel.RefreshComputerSharesCommand.ExecuteAsync(null);
    }

    private readonly NetworkViewModel _viewModel;
}