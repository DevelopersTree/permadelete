using RudeFox.Helpers;
using RudeFox.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RudeFox.ViewModels
{
    class AboutDialogVM : BindableBase
    {
        #region Constructor
        public AboutDialogVM()
        {
            VisitWebsiteCommand = new DelegateCommand(p => Process.Start(Constants.WEBSITE_URL));
        }
        #endregion

        #region Properties
        public string Version
        {
            get
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // return "VERSION " + version.Major.ToString() + "." + version.Minor.ToString();
                return "VERSION " + version.ToString();
            }
        }
        #endregion

        #region Commands
        public ICommand VisitWebsiteCommand { get; private set; }
        #endregion
    }
}
