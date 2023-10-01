using System.Windows;

namespace KVideoLauncher.Data;

public class ShareSelectedEventArgs : RoutedEventArgs
{
    public string ComputerShare { get; }

    public ShareSelectedEventArgs(RoutedEvent routedEvent, object source, string computerShare)
        : base(routedEvent, source) => ComputerShare = computerShare;
}