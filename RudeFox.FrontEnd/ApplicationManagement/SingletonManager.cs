using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;
using RudeFox.Helpers;
using RudeFox.Views;
using RudeFox.Services;
using NLog.Targets;
using RudeFox.Updater;

namespace RudeFox.ApplicationManagement
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
            App.Instance.RegisterExceptionHandlingEvents();
            InitializeComponents();
            App.Instance.Run();
            return false;
        }

        protected override async void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // subsequent launches
            Task deleteTask = App.Instance.ProcessCommandLineArgs(eventArgs.CommandLine?.ToList());

            App.Instance.Activate();

            await deleteTask;
        }

        private void InitializeComponents()
        {
            // register sentry as NLog target
            Target.Register<Nlog.SentryTarget>("Sentry");

#if !DEBUG
            // check for updates
            UpdateManager.Initialize(Keys.DROPBOX_API_KEY);
            Task.Run(() => App.Instance.UpdateAfter(5));
#endif
        }
    }
}
