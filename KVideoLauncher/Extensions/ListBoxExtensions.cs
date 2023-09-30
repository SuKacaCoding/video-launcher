using System.Windows;
using System.Windows.Controls;

namespace KVideoLauncher.Extensions;

internal static class ListBoxExtensions
{
    public static void FocusOnSelectionOrItself(this ListBox listBox)
    {
        if (listBox.SelectedIndex == -1)
            listBox.Focus();
        else
            FocusOnSelection(listBox);
    }

    public static void FocusOnSelection(this ListBox listBox)
    {
        (listBox.ItemContainerGenerator.ContainerFromIndex(listBox.SelectedIndex) as FrameworkElement)?.Focus();
    }
}