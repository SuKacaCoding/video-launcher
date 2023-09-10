using KVideoLauncher.Properties.Lang;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace KVideoLauncher.Helpers;

public static class ExceptionDisplayingHelper
{
    public static void Display(Exception ex)
    {
        Display(ex.Message);
    }

    public static void Display(string message)
    {
        MessageBox.Show(message, Labels.Exception, icon: MessageBoxImage.Error);
    }
}