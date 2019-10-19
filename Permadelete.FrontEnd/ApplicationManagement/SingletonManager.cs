using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;
using Permadelete.Helpers;
using Permadelete.Views;
using Permadelete.Services;
using NLog.Targets;
using Permadelete.Updater;
using Permadelete.Enums;

namespace Permadelete.ApplicationManagement
{
    public class SingletonManager : WindowsFormsApplicationBase
    {
        public SingletonManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            // first time app is launched
            App.Current.RegisterExceptionHandlingEvents();
            InitializeComponents();
            App.Current.Run();
            return false;
        }

        protected override async void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // subsequent launches
            App.Current.Activate();

            var settings = SettingsHelper.GetSettings();

            if (eventArgs.CommandLine.Count() > 0)
                await App.Current.DeleteFilesOrFolders(eventArgs.CommandLine);
        }

        private void InitializeComponents()
        {
#if CLASSIC
            if (string.IsNullOrWhiteSpace(Keys.DROPBOX_API_KEY))
                DialogService.GetMessageDialog("API key not found", "Could not find Dropbox API key.", "Ok").ShowDialog();
            else
            {
                UpdateManager.Initialize(Keys.DROPBOX_API_KEY);
                Task.Run(() => App.Current.UpdateAfter(5));
            }
#endif
        }
    }
}
