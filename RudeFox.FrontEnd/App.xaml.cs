using System;
using System.Threading.Tasks;
using System.Windows;
using RudeFox.Views;
using RudeFox.Services;
using NLog.Config;
using RudeFox.Updater;
using System.Reflection;

namespace RudeFox.FrontEnd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // handle the unhandled global exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => LogUnhandledException((Exception)args.ExceptionObject);
            DispatcherUnhandledException += (sender, args) => LogUnhandledException(args.Exception);
            TaskScheduler.UnobservedTaskException += (s, args) => LogUnhandledException(args.Exception);

            // register sentry as NLog target
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("Sentry", typeof(Nlog.SentryTarget));

            // check for updates
            UpdateManager.Initialize(Keys.DROPBOX_API_KEY);
            Task.Run(() => UpdateAfter(5));

            // open the main window
            var window = new MainWindow();
            this.MainWindow = window;
            window.Show();
        }

        private void LogUnhandledException(Exception e)
        {
            LoggerService.Instance.Error(e);
            DialogService.Instance.GetErrorDialog("An unxpected error occured", e).ShowDialog();
        }

        /// <summary>
        /// Checks for updates, if there was a newer version returns true. Otherwise, it will return false.
        /// However, if there was an error during the update, it will return null.
        /// </summary>
        /// <param name="seconds">Seconds to wait before checking for update</param>
        /// <returns></returns>
        internal async Task<bool?> UpdateAfter(int seconds)
        {
            await Task.Delay(seconds * 1000);

            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var tempFolder = await UpdateManager.DownloadLatestUpdate(version);
                if (tempFolder == null) return false; // no update

                await Current.Dispatcher.BeginInvoke(new Action(() => 
                    Current.Exit += (sender, args) => UpdateManager.ApplyUpdate(tempFolder)
                ));
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Error(ex);
                return null; // error occured
            }

            return true; // updated and waiting for the app to exit
        }
    }
}
