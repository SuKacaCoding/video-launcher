using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools;
using Window = HandyControl.Controls.Window;

namespace KVideoLauncher.Views;

public partial class MainWindow : Window
{
    private ListBox FocusedListBox
    {
        get => _focusableListBoxes[_focusedListBoxIndex];
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
            _focusedListBoxIndex = value % _focusableListBoxes.Count;
            if (_focusedListBoxIndex < 0)
                _focusedListBoxIndex += _focusableListBoxes.Count;
            FocusOnListBox(FocusedListBox);
        }
    }

    private const int ListBoxWideMoveOffset = 5;

    private static readonly GridLength SelectedColumnWidth = new(value: 3, GridUnitType.Star);
    private static readonly GridLength GeneralColumnWidth = new(value: 1, GridUnitType.Star);

    public MainWindow()
    {
        InitializeComponent();

        this.Apply(BackdropType.Acrylic);
        
        _widthAdjustableColumnByListBox = new Dictionary<ListBox, ColumnDefinition>
        {
            { ListDirectories, ColDirectories },
            { ListFiles, ColFiles },
            { ListPlaylist, ColPlaylist }
        }.AsReadOnly();

        _focusableListBoxes = new List<ListBox>
        {
            ListDrives,
            ListFrequent,
            ListDirectories,
            ListFiles,
            ListPlaylist
        }.AsReadOnly();

        // Initialize Commands.
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.OemOpenBrackets, ModifierKeys.Control),
                Command = MoveListBoxFocusLeftCommand
            }
        );
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.OemCloseBrackets, ModifierKeys.Control),
                Command = MoveListBoxFocusRightCommand
            }
        );
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.N, modifiers: ModifierKeys.Control | ModifierKeys.Shift),
                Command = WideMoveFocusedListBoxSelectionDownCommand
            }
        );
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.P, modifiers: ModifierKeys.Control | ModifierKeys.Shift),
                Command = WideMoveFocusedListBoxSelectionUpCommand
            }
        );
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.N, ModifierKeys.Control),
                Command = PreciselyMoveFocusedListBoxSelectionDownCommand
            }
        );
        InputBindings.Add
        (
            new KeyBinding
            {
                Gesture = new KeyGesture(Key.P, ModifierKeys.Control),
                Command = PreciselyMoveFocusedListBoxSelectionUpCommand
            }
        );

        FocusedListBoxIndex = 0;
    }

    private void ListBoxMouseDown(object sender, RoutedEventArgs e)
    {
        if (e.Source is ListBox listBox)
            FocusedListBox = listBox;
    }

    private int FocusOnListBox(ListBox listBox)
    {
        int indexOfListBox = _focusableListBoxes.IndexOf(listBox);
        if (indexOfListBox == -1)
            return -1;

        if (listBox.SelectedIndex == -1)
            listBox.Focus();
        else
            FocusOnListBoxSelection(listBox);

        foreach (var column in _widthAdjustableColumnByListBox.Values)
            column.Width = GeneralColumnWidth;

        if (_widthAdjustableColumnByListBox.TryGetValue(listBox, value: out var selectedColumn))
            selectedColumn.Width = SelectedColumnWidth;

        return indexOfListBox;
    }

    [RelayCommand]
    private void MoveListBoxFocusLeft()
    {
        FocusedListBoxIndex--;
    }

    [RelayCommand]
    private void MoveListBoxFocusRight()
    {
        FocusedListBoxIndex++;
    }

    [RelayCommand]
    private void WideMoveFocusedListBoxSelectionDown()
    {
        CommonMoveFocusedListBoxSelection(ListBoxWideMoveOffset);
    }

    [RelayCommand]
    private void WideMoveFocusedListBoxSelectionUp()
    {
        CommonMoveFocusedListBoxSelection(-ListBoxWideMoveOffset);
    }

    [RelayCommand]
    private void PreciselyMoveFocusedListBoxSelectionDown()
    {
        CommonMoveFocusedListBoxSelection(1);
    }

    [RelayCommand]
    private void PreciselyMoveFocusedListBoxSelectionUp()
    {
        CommonMoveFocusedListBoxSelection(-1);
    }

    private void CommonMoveFocusedListBoxSelection(int offset)
    {
        int targetSelectedIndex = FocusedListBox.SelectedIndex + offset;
        int itemsCount = FocusedListBox.Items.Count;

        if (targetSelectedIndex >= itemsCount)
            targetSelectedIndex -= itemsCount;
        else if (targetSelectedIndex < 0)
            targetSelectedIndex += itemsCount;

        FocusedListBox.SelectedIndex = targetSelectedIndex;
        FocusOnListBoxSelection(FocusedListBox);
    }

    private static void FocusOnListBoxSelection(ListBox listBox)
    {
        (listBox.ItemContainerGenerator.ContainerFromIndex(listBox.SelectedIndex) as FrameworkElement)?.Focus();
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    private readonly ReadOnlyCollection<ListBox> _focusableListBoxes;
    private readonly ReadOnlyDictionary<ListBox, ColumnDefinition> _widthAdjustableColumnByListBox;

    private int _focusedListBoxIndex;
}