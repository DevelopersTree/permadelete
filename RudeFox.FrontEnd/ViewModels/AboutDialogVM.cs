using RudeFox.Helpers;
using RudeFox.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using RudeFox.ApplicationManagement;
using RudeFox.Enums;

namespace RudeFox.ViewModels
{
    class AboutDialogVM : BindableBase
    {
        #region Constructor
        public AboutDialogVM()
        {
            VisitWebsiteCommand = new DelegateCommand(p => Process.Start(Constants.WEBSITE_URL));

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        #endregion

        #region Fields
        private UpdateStatus _lastStatus;
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
            set { SetProperty(ref _showBusyIndicator, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }
        #endregion

        #region Commands
        public ICommand VisitWebsiteCommand { get; private set; }
        #endregion

        #region Methods
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (App.Current.UpdateStatus == _lastStatus)
                return;

            ShowBusyIndicator = App.Current.UpdateStatus == UpdateStatus.CheckingForUpdate ||
                                App.Current.UpdateStatus == UpdateStatus.DownloadingUpdate;

            switch (App.Current.UpdateStatus)
            {
                case UpdateStatus.Idle:
                    StatusMessage = string.Empty;
                    break;
                case UpdateStatus.CheckingForUpdate:
                    StatusMessage = "Checking for update...";
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

            _lastStatus = App.Current.UpdateStatus;
        }
        #endregion
    }
}
