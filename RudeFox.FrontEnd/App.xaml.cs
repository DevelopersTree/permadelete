using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RudeFox.Views;
using RudeFox.Services;

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

            // open the main window
            var window = new MainWindow();
            this.MainWindow = window;
            window.Show();
        }

        private void LogUnhandledException(Exception e)
        {
            LoggerService.Instance.Error(e);
            DialogService.Instance.GetErrorDialog("An unxpected error occured.", e);
        }
    }
}
