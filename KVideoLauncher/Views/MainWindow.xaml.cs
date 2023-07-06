using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace KVideoLauncher.Views;

public partial class MainWindow : Window
{
    private static readonly GridLength SelectedColumnWidth = new(value: 2, GridUnitType.Star);
    private static readonly GridLength GeneralColumnWidth = new(value: 1, GridUnitType.Star);

    public MainWindow()
    {
        InitializeComponent();

        _widthAdjustableColumns = new ReadOnlyCollection<ColumnDefinition>
        (
            new List<ColumnDefinition>
            {
                ColDirectories,
                ColVideos,
                ColPlaylist
            }
        );

        AddHandler(PreviewMouseDownEvent, handler: new RoutedEventHandler(ListBoxMouseDown));
    }

    private void ListBoxMouseDown(object sender, RoutedEventArgs e)
    {
        if (e.Source is not ListBox listBox)
            return;
        if (listBox == ListPath)
            return;

        FocusOnListBox(listBox);
    }

    private void FocusOnListBox(ListBox listBox)
    {
        listBox.Focus();
        foreach (var column in _widthAdjustableColumns)
            column.Width = GeneralColumnWidth;

        if (listBox == ListDirectories)
            ColDirectories.Width = SelectedColumnWidth;
        else if (listBox == ListVideos)
            ColVideos.Width = SelectedColumnWidth;
        else if (listBox == ListPlaylist)
            ColPlaylist.Width = SelectedColumnWidth;
    }

    private readonly ReadOnlyCollection<ColumnDefinition> _widthAdjustableColumns;
}