using Microsoft.Win32;
using System;
using System.Windows;

namespace LaunchBoxRomPatchManager.Helpers
{
    public class MessageDialogHelper
    {
        public static MessageDialogResult ShowOKCancelDialog(string text, string title)
        {
            return MessageBox.Show(text, title, MessageBoxButton.OKCancel) == MessageBoxResult.OK
                ? MessageDialogResult.OK
                : MessageDialogResult.Cancel;
        }
    }

    public enum MessageDialogResult
    {
        OK,
        Cancel
    }
}
