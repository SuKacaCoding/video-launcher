using KVideoLauncher.Data;
using KVideoLauncher.Extensions;
using KVideoLauncher.ViewModels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KVideoLauncher.Views;

public partial class NetworkView : UserControl
{
    public static readonly RoutedUICommand OkCommand = new();

    public static readonly RoutedEvent ShareSelectedEvent = EventManager.RegisterRoutedEvent
    (
        name: nameof(ShareSelected), RoutingStrategy.Bubble, handlerType: typeof(EventHandler<ShareSelectedEventArgs>),
        ownerType: typeof(NetworkView)
    );

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

    private void OkCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        OnShareSelected();
    }

    private void OnShareSelected()
    {
        var e = new ShareSelectedEventArgs
            (ShareSelectedEvent, source: this, computerShare: (string)ListComputerShares.SelectedItem);
        RaiseEvent(e);
    }

    public event EventHandler<ShareSelectedEventArgs> ShareSelected
    {
        add => AddHandler(ShareSelectedEvent, value);
        remove => RemoveHandler(ShareSelectedEvent, value);
    }

    private readonly NetworkViewModel _viewModel;
}