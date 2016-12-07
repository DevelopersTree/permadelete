using System;
using System.Threading.Tasks;
using System.Windows;
using RudeFox.Views;
using RudeFox.Services;
using NLog.Config;

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
    }
}
