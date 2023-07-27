using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace KVideoLauncher.Helpers;

public static class ExceptionDisplayHelper
{
    public static void Display(Exception ex)
    {
        MessageBox.Show(ex.Message, caption: "Exception", icon: MessageBoxImage.Error);
    }
}