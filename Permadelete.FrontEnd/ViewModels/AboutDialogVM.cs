using Permadelete.Helpers;
using Permadelete.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Permadelete.ApplicationManagement;
using Permadelete.Enums;

namespace Permadelete.ViewModels
{
    class AboutDialogVM : BindableBase
    {
        #region Constructor
        public AboutDialogVM()
        {
            VisitWebsiteCommand = new DelegateCommand(p => Process.Start(Constants.WEBSITE_URL));

            App.Current.UpdateStatusChanged += UpdateStatus_Changed;
            UpdateStatus_Changed(null, new EventArgs());
        }
        #endregion

        #region Properties
        public string Version
        {
            get
            {
               return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Shorten();
            }
        }

        private bool _showBusyIndicator;
        public bool ShowBusyIndicator
        {
            get { return _showBusyIndicator; }
            set { Set(ref _showBusyIndicator, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { Set(ref _statusMessage, value); }
        }

        public string Title { get { return Constants.APPLICATION_NAME; } }
        #endregion

        #region Commands
        public ICommand VisitWebsiteCommand { get; private set; }
        #endregion

        #region Methods
        private void UpdateStatus_Changed(object sender, EventArgs e)
        {
            ShowBusyIndicator = App.Current.UpdateStatus == UpdateStatus.CheckingForUpdate ||
                                App.Current.UpdateStatus == UpdateStatus.DownloadingUpdate;

            switch (App.Current.UpdateStatus)
            {
                case UpdateStatus.Idle:
                    StatusMessage = string.Empty;
                    break;
                case UpdateStatus.CheckingForUpdate:
                    StatusMessage = "Checking for updates...";
                    break;
                case UpdateStatus.DownloadingUpdate:
                    StatusMessage = "Downloading new update...";
                    break;
                case UpdateStatus.UpdateDownloaded:
                    StatusMessage = "Update downloaded. It will take effect next time your start the app :)";
                    break;
                case UpdateStatus.LatestVersion:
                    StatusMessage = "Awesome! You are using the latest version.";
                    break;
            }
        }
        #endregion
    }
}
