using Permadelete.Enums;
using Permadelete.ViewModels;
using Permadelete.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Permadelete.Services
{
    public static class DialogService
    {

        #region Methods
        public static ErrorDialog GetErrorDialog(string title, Exception exception)
        {
            var window = new ErrorDialog();
            window.DataContext = new ErrorDialogVM(title, exception.Message, exception);
            return window;
        }

        public static SettingsDialog GetSettingsDialog()
        {
            return new SettingsDialog();
        }

        public static AboutDialog GetAboutDialog()
        {
            var window = new AboutDialog();
            return window;
        }

        public static ConfirmDialog GetConfirmDialog(string message)
        {
            return new ConfirmDialog(message);
        }

        public static MessageDialog GetMessageDialog(string title, string message, string okButton)
        {
            return GetMessageDialog(title, message, MessageIcon.Information, okButton, null, false);
        }

        public static MessageDialog GetMessageDialog(string title, string message, MessageIcon icon, string okButton, string cancelButton, bool isDestructive)
        {
            var dataContext = new MessageDialogVM(title, message, icon, okButton, cancelButton, isDestructive);
            var window = new MessageDialog();
            window.DataContext = dataContext;
            return window;
        }
        #endregion
    }
}
