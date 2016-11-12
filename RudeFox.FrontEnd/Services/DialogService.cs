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
        public bool? OpenAboutDialog()
        {
            var window = new AboutDialog();
            return window.ShowDialog();
        }
        #endregion
    }
}
