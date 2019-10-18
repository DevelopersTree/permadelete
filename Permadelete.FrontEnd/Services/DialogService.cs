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
    public sealed class DialogService
    {
        #region Constructor
        private DialogService()
        {

        }
        #endregion

        #region Properties
        // using Lazy<T> here makes the the field both lazily loaded and thread safe
        // more info: http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly Lazy<DialogService> _instance = new Lazy<DialogService>(() => new DialogService());
        public static DialogService Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region Methods
        public ErrorDialog GetErrorDialog(string title, Exception exception)
        {
            var window = new ErrorDialog();
            window.DataContext = new ErrorDialogVM(title, exception.Message, exception);
            return window;
        }

        public SettingsDialog GetSettingsDialog()
        {
            return new SettingsDialog();
        }

        public AboutDialog GetAboutDialog()
        {
            var window = new AboutDialog();
            return window;
        }

        public MessageDialog GetMessageDialog(string title, string message, string okButton)
        {
            return GetMessageDialog(title, message, MessageIcon.Information, okButton, null, false);
        }

        public MessageDialog GetMessageDialog(string title, string message, MessageIcon icon, string okButton, string cancelButton, bool isDestructive)
        {
            var dataContext = new MessageDialogVM(title, message, icon, okButton, cancelButton, isDestructive);
            var window = new MessageDialog();
            window.DataContext = dataContext;
            return window;
        }
        #endregion
    }
}
