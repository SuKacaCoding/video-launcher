using System.Windows;
using KVideoLauncher.Properties.Lang;
using MessageBox = HandyControl.Controls.MessageBox;

namespace KVideoLauncher.Helpers;

public static class ExceptionDisplayHelper
{
    public static void Display(Exception ex)
    {
        MessageBox.Show(ex.Message, Labels.Exception, icon: MessageBoxImage.Error);
    }
}