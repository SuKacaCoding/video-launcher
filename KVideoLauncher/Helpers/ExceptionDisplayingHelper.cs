using KVideoLauncher.Properties.Lang;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace KVideoLauncher.Helpers;

public static class ExceptionDisplayingHelper
{
    public static void Display(Exception ex)
    {
        MessageBox.Show(ex.Message, Labels.Exception, icon: MessageBoxImage.Error);
    }
}