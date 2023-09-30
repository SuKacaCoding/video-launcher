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
    }

    public async Task PrepareAsync()
    {
        ListComputerShares.FocusOnSelectionOrItself();

        await ((NetworkViewModel)DataContext).RefreshComputerSharesCommand.ExecuteAsync(null);
    }
}