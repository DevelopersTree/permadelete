using System;
using System.Threading.Tasks;
using System.Windows;
using RudeFox.Views;
using RudeFox.Services;
using NLog.Config;
using RudeFox.Updater;
using System.Reflection;
using System.Collections.ObjectModel;
using RudeFox.ViewModels;
using NLog.Targets;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using RudeFox.Models;
using RudeFox.Helpers;

namespace RudeFox.FrontEnd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructors
        public App()
        {
            Operations = new ReadOnlyObservableCollection<OperationVM>(_operationsSource);
        }
        #endregion

        #region Properties
        private static ObservableCollection<OperationVM> _operationsSource = new ObservableCollection<OperationVM>();
        public static ReadOnlyObservableCollection<OperationVM> Operations { get; private set; }
        #endregion

        #region OnStartup
        protected override async void OnStartup(StartupEventArgs e)
        {
            RegisterExceptionHandlingEvents();
            InitializeComponents();

            Task deleteTask = null;
            if (e.Args.Length > 1 && e.Args[0].Equals(Constants.SENDTO_PREFIX, StringComparison.InvariantCultureIgnoreCase))
                deleteTask = DeleteFilesOrFolders(e.Args.ToList());

            // open the main window
            var window = new MainWindow();
            this.MainWindow = window;
            window.Show();

            if (deleteTask != null)
                await deleteTask;
        }
        #endregion

        #region Methods
        private static void AddOperation(OperationVM operation)
        {
            _operationsSource.Add(operation);
        }

        public static void RemoveOperation(OperationVM operation)
        {
            _operationsSource.Remove(operation);
        }

        public static async Task DeleteFileOrFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path should not be null or empty.");

            var operation = new OperationVM { Path = path };
            AddOperation(operation);

            var task = ShredderService.Instance.ShredItemAsync(operation.Path, operation.CancellationTokenSource.Token, operation.TaskProgress);
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception exc)
            {
                LoggerService.Instance.Error(exc);
                DialogService.Instance.GetErrorDialog("Could not delete item", exc).ShowDialog();
            }
            finally
            {
                await Current.Dispatcher.InvokeAsync(() => RemoveOperation(operation));
            }
        }

        public static async Task DeleteFilesOrFolders(List<string> paths)
        {
            paths.Remove(Constants.SENDTO_PREFIX);
            var userAgreed = await GetUserAgreedToDeleteAsync(paths);
            if (userAgreed != true) return;

            var duplicates = Operations.Select(item => item.Path).Intersect(paths);
            paths.RemoveAll(p => duplicates.Contains(p));

            var validPaths = paths.Where(path => System.IO.File.Exists(path) || Directory.Exists(path));
            var tasks = validPaths.Select(item => DeleteFileOrFolder(item)).ToList();

            await Task.WhenAll(tasks);
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

        private void LogUnhandledException(Exception e)
        {
            LoggerService.Instance.Error(e);
            DialogService.Instance.GetErrorDialog("An unxpected error occured", e).ShowDialog();
        }

        private static async Task<bool?> GetUserAgreedToDeleteAsync(List<string> paths)
        {
            string message;
            string okText = "Delete ";
            var itemName = System.IO.File.Exists(paths[0]) ? "file" : "folder";

            if (paths.Count == 1)
            {
                message = $"Are you sure you want to delete this {itemName}?{Environment.NewLine}";
                message += Path.GetFileName(paths[0]);
                okText += "it";
            }
            else
            {
                message = $"Are you sure you want to delete these {paths.Count} items?";
                okText += "them";
            }

            return await DialogService.Instance.GetMessageDialog("Deleting items", message, MessageIcon.Exclamation, okText, "Cancel", true).ShowDialogAsync();
        }

        private void InitializeComponents()
        {
            // register sentry as NLog target
            Target.Register<Nlog.SentryTarget>("Sentry");

#if !DEBUG
            // check for updates
            UpdateManager.Initialize(Keys.DROPBOX_API_KEY);
            Task.Run(() => UpdateAfter(5));
#endif
        }

        private void RegisterExceptionHandlingEvents()
        {
            // handle the unhandled global exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => LogUnhandledException((Exception)args.ExceptionObject);
            DispatcherUnhandledException += (sender, args) => LogUnhandledException(args.Exception);
            TaskScheduler.UnobservedTaskException += (s, args) => LogUnhandledException(args.Exception);
        }
        #endregion
    }
}
