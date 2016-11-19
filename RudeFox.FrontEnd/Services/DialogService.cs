using RudeFox.Models;
using RudeFox.ViewModels;
using RudeFox.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RudeFox.Services
{
    public class DialogService
    {
        #region Constructor
        private DialogService()
        {

        }
        #endregion

        #region Properties
        private static DialogService _instance;
        public static DialogService Instance
        {
            get { return _instance ?? (_instance = new DialogService()); }
        }
        #endregion

        #region Methods
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
