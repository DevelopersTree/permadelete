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
using RudeFox.Enums;
using RudeFox.Helpers;

namespace RudeFox.ApplicationManagement
{
    public class App : Application
    {
        #region Constructors
        private App()
        {
            Operations = new ReadOnlyObservableCollection<OperationVM>(_operationsSource);
        }
        #endregion

        #region Properties
        private static ObservableCollection<OperationVM> _operationsSource = new ObservableCollection<OperationVM>();
        public static ReadOnlyObservableCollection<OperationVM> Operations { get; private set; }

        private static readonly Lazy<App> _instance = new Lazy<App>(() => new App());
        public static App Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region OnStartup
        protected override async void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Count() == 0)
            {
                this.MainWindow = new MainWindow();
                this.MainWindow.Show();
            }
            else
            {
                var window = new AgileWindow();
                window.DataContext = new AgileWindowVM(e.Args.ToList());
                await window.ShowDialogAsync();
            }
        }

        #endregion

        #region Methods
        public async Task DeleteFilesOrFolders(List<string> paths, bool silent = false)
        {
            if (!silent)
            {
                var userAgreed = await GetUserAgreedToDeleteAsync(paths);
                if (userAgreed != true) return;
            }

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

                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    Application.Current.Exit += (sender, args) => UpdateManager.ApplyUpdate(tempFolder)
                ));
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Error(ex);
                return null; // error occured
            }

            return true; // updated and waiting for the app to exit
        }

        internal void Activate()
        {
            MainWindow.Activate();
            if (MainWindow.WindowState == WindowState.Minimized)
                MainWindow.WindowState = WindowState.Normal;
        }

        internal Task ProcessCommandLineArgs(List<string> args)
        {
            if (args == null) return null;

            Task deleteTask = Task.Delay(0); // do nothing
            if (args.Count > 1 && args[0].Equals(Constants.SENDTO_PREFIX, StringComparison.InvariantCultureIgnoreCase))
            {
                args.Remove(Constants.SENDTO_PREFIX);
                deleteTask = DeleteFilesOrFolders(args);
            }

            return deleteTask;
        }

        internal void RegisterExceptionHandlingEvents()
        {
            // handle the unhandled global exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => LogUnhandledException((Exception)args.ExceptionObject);
            App.Instance.DispatcherUnhandledException += (sender, args) => LogUnhandledException(args.Exception);
            TaskScheduler.UnobservedTaskException += (s, args) => LogUnhandledException(args.Exception);
        }

        private async Task DeleteFileOrFolder(string path)
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
                await Application.Current.Dispatcher.InvokeAsync(() => RemoveOperation(operation));
            }
        }

        private void AddOperation(OperationVM operation) => _operationsSource.Add(operation);

        public void RemoveOperation(OperationVM operation) => _operationsSource.Remove(operation);

        private async Task<bool?> GetUserAgreedToDeleteAsync(List<string> paths)
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
            var dialog = DialogService.Instance.GetMessageDialog("Deleting items", message, MessageIcon.Exclamation, okText, "Cancel", true);
            dialog.Owner = this.MainWindow;
            return await dialog.ShowDialogAsync();
        }

        private void LogUnhandledException(Exception e)
        {
            LoggerService.Instance.Error(e);
            DialogService.Instance.GetErrorDialog("An unxpected error occured", e).ShowDialog();
        }
        #endregion
    }
}
