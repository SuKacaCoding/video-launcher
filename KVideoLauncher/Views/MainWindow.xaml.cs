using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Window = HandyControl.Controls.Window;

namespace KVideoLauncher.Views;

public partial class MainWindow : Window
{
    private ListBox FocusedListBox
    {
        get => _verticalListBoxes[_focusedListBoxIndex];
        set
        {
            int indexOfValue = FocusOnListBox(value);
            _focusedListBoxIndex = indexOfValue == -1 ? _focusedListBoxIndex : indexOfValue;
        }
    }

    private int FocusedListBoxIndex
    {
        get => _focusedListBoxIndex;
        set
        {
            _focusedListBoxIndex = value % _verticalListBoxes.Count;
            if (_focusedListBoxIndex < 0)
                _focusedListBoxIndex += _verticalListBoxes.Count;
            FocusOnListBox(FocusedListBox);
        }
    }

    private ICommand MoveListBoxFocusLeftCommand { get; }
    private ICommand MoveListBoxFocusRightCommand { get; }

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
        _verticalListBoxes = new ReadOnlyCollection<ListBox>
        (
            new List<ListBox>
            {
                ListDrives,
                ListDirectories,
                ListVideos,
                ListPlaylist
            }
        );

        FocusedListBoxIndex = 0;

        // Initialize Commands.
        MoveListBoxFocusLeftCommand = new RelayCommand(MoveListBoxFocusLeft);
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.OemOpenBrackets, ModifierKeys.Control),
                Command = MoveListBoxFocusLeftCommand
            }
        );
        MoveListBoxFocusRightCommand = new RelayCommand(MoveListBoxFocusRight);
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.OemCloseBrackets, ModifierKeys.Control),
                Command = MoveListBoxFocusRightCommand
            }
        );
    }

    private void ListBoxMouseDown(object sender, RoutedEventArgs e)
    {
        if (e.Source is ListBox listBox)
            FocusedListBox = listBox;
    }

    private int FocusOnListBox(ListBox listBox)
    {
        int indexOfListBox = _verticalListBoxes.IndexOf(listBox);
        if (indexOfListBox == -1)
            return -1;

        listBox.Focus();

        foreach (var column in _widthAdjustableColumns)
            column.Width = GeneralColumnWidth;

        if (listBox == ListDirectories)
            ColDirectories.Width = SelectedColumnWidth;
        else if (listBox == ListVideos)
            ColVideos.Width = SelectedColumnWidth;
        else if (listBox == ListPlaylist)
            ColPlaylist.Width = SelectedColumnWidth;

        return indexOfListBox;
    }

    private void MoveListBoxFocusLeft()
    {
        FocusedListBoxIndex--;
    }

    private void MoveListBoxFocusRight()
    {
        FocusedListBoxIndex++;
    }

    private readonly ReadOnlyCollection<ListBox> _verticalListBoxes;

    private readonly ReadOnlyCollection<ColumnDefinition> _widthAdjustableColumns;
    private int _focusedListBoxIndex;
}