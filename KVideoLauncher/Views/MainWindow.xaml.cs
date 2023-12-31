﻿using CommunityToolkit.Mvvm.Input;
using HandyControl.Tools;
using KVideoLauncher.Data;
using KVideoLauncher.Extensions;
using KVideoLauncher.Helpers;
using KVideoLauncher.Properties;
using KVideoLauncher.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            _focusedListBoxIndex = value.MathMod(_focusableListBoxes.Count);
            FocusOnListBox(FocusedListBox);
        }
    }

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = (MainWindowViewModel)DataContext;

        #region Apply Acrylic effect

        if (this.Apply(BackdropType.Acrylic))
            Background = FindResource("TranslucentBackgroundBrush") as Brush;

        #endregion Apply Acrylic effect

        #region Initialize fields

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

        #endregion Initialize fields

        #region Initialize commands

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

        #endregion Initialize commands

        #region Initialize access keys

        AccessKeyManager.Register(key: "e", TglBtnNetwork);

        #endregion Initialize access keys

        FocusedListBoxIndex = 0;
    }

    private void CommonMoveFocusedListBoxSelection(int offset)
    {
        var targetListBox = DrawerLeft.IsOpen ? MyNetworkView.ListComputerShares : FocusedListBox;

        int itemsCount = targetListBox.Items.Count;
        if (itemsCount == 0)
            return;

        int targetSelectedIndex = targetListBox.SelectedIndex + offset;

        targetSelectedIndex = targetSelectedIndex.MathMod(itemsCount);

        targetListBox.SelectedIndex = targetSelectedIndex;
        targetListBox.FocusOnSelection();
    }

    private void DrawerClosed(object sender, RoutedEventArgs e)
    {
        FocusedListBox.FocusOnSelectionOrItself();
    }

    private async void DrawerOpened(object sender, RoutedEventArgs e)
    {
        await MyNetworkView.PrepareAsync();
    }

    private int FocusOnListBox(ListBox listBox)
    {
        int indexOfListBox = _focusableListBoxes.IndexOf(listBox);
        if (indexOfListBox == -1)
            return -1;

        listBox.FocusOnSelectionOrItself();

        foreach (var column in _widthAdjustableColumnByListBox.Values)
            column.Width = GeneralColumnWidth;

        if (_widthAdjustableColumnByListBox.TryGetValue(listBox, value: out var selectedColumn))
            selectedColumn.Width = SelectedColumnWidth;

        return indexOfListBox;
    }

    private void ListBoxMouseDown(object sender, RoutedEventArgs e)
    {
        if (e.Source is ListBox listBox)
            FocusedListBox = listBox;
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        Settings.Default.WindowPosition = RestoreBounds.ToString(CultureInfo.InvariantCulture);
        Settings.Default.Save();
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

    private void MyNetworkViewShareSelected(object? sender, ShareSelectedEventArgs e)
    {
        DrawerLeft.IsOpen = false;

        IAsyncRelayCommand<string?> changeDriveCommand = _viewModel.ChangeDriveCommand;
        if (changeDriveCommand.CanExecute(null))
            changeDriveCommand.ExecuteAsync(e.ComputerShare);
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

    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var bounds = Rect.Parse(Settings.Default.WindowPosition);
            Top = bounds.Top;
            Left = bounds.Left;

            WindowState = WindowState.Maximized;
        }
        catch (Exception ex)
        {
            ExceptionDisplayingHelper.Display(ex);
        }
    }

    private const int ListBoxWideMoveOffset = 5;

    private static readonly GridLength GeneralColumnWidth = new(value: 1, GridUnitType.Star);
    private static readonly GridLength SelectedColumnWidth = new(value: 3, GridUnitType.Star);
    private readonly ReadOnlyCollection<ListBox> _focusableListBoxes;
    private readonly MainWindowViewModel _viewModel;
    private readonly ReadOnlyDictionary<ListBox, ColumnDefinition> _widthAdjustableColumnByListBox;

    private int _focusedListBoxIndex;
}