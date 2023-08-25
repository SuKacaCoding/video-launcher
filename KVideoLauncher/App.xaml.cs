using KVideoLauncher.Views;
using System.Windows;

namespace KVideoLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App() => InitializeComponent();

    public void Activate()
    {
        MainWindow?.Activate();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new MainWindow();
        window.Show();
    }
}