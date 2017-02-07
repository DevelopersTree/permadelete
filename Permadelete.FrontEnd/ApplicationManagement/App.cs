using System;
using System.Threading.Tasks;
using System.Windows;
using Permadelete.Views;
using Permadelete.Services;
using NLog.Config;
using Permadelete.Updater;
using System.Reflection;
using System.Collections.ObjectModel;
using Permadelete.ViewModels;
using NLog.Targets;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Permadelete.Enums;
namespace Permadelete.ApplicationManagement
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

        private static readonly Lazy<App> _current = new Lazy<App>(() => new App());
        public static new App Current
        {
            get { return _current.Value; }
        }

        public event EventHandler UpdateStatusChanged;

        private UpdateStatus _updateStatus;
        public UpdateStatus UpdateStatus
        {
            get { return _updateStatus; }
            set
            {
                if (_updateStatus == value) return;

                _updateStatus = value;
                OnUpdateStatusChanged();
            }
        }
        #endregion

        #region Events
        public event EventHandler<NotificationEventArgs> NotificationRaised;
        #endregion

        #region OnStartup
        protected override void OnStartup(StartupEventArgs e)
        {
            var stylesDic = new Uri("pack://application:,,,/Styles.xaml");
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = stylesDic });

            if (e.Args.Count() == 0)
            {
                this.MainWindow = new MainWindow();
                this.MainWindow.Show();
            }
            else
            {
                var window = new AgileWindow();
                window.DataContext = new AgileWindowVM(e.Args);
                window.Show();
            }

        }

        #endregion

        #region Methods
        public async Task DeleteFilesOrFolders(IEnumerable<string> paths, bool silent = false)
        {
            if (!silent)
            {
                var userAgreed = await GetUserAgreedToDeleteAsync(paths);
                if (userAgreed != true) return;
            }

            var duplicates = Operations.Select(item => item.Path).Intersect(paths);
            paths = paths.Except(duplicates);

            var validPaths = paths.Where(path => System.IO.File.Exists(path) || Directory.Exists(path));
            var tasks = validPaths.Select(item => ShredFileOrFolder(item));

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
            UpdateStatus = UpdateStatus.Idle;
            await Task.Delay(seconds * 1000);
            UpdateStatus = UpdateStatus.CheckingForUpdate;

            try
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;

                var updateInfo = await UpdateManager.CheckForUpdates().ConfigureAwait(false);
                if (updateInfo == null || updateInfo.Version <= version || updateInfo?.Path == null)
                {
                    UpdateStatus = UpdateStatus.LatestVersion;
                    return false;
                }

                UpdateStatus = UpdateStatus.DownloadingUpdate;
                var tempFolder = await UpdateManager.DownloadLatestUpdate(version, updateInfo).ConfigureAwait(false);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                    Application.Current.Exit += (sender, args) => UpdateManager.ApplyUpdate(tempFolder)
                ));

                UpdateStatus = UpdateStatus.UpdateDownloaded;
            }
            catch (Exception ex)
            {
                UpdateStatus = UpdateStatus.Idle;
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

        internal void RegisterExceptionHandlingEvents()
        {
            // handle the unhandled global exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => LogUnhandledException((Exception)args.ExceptionObject);
            App.Current.DispatcherUnhandledException += (sender, args) => LogUnhandledException(args.Exception);
            TaskScheduler.UnobservedTaskException += (s, args) => LogUnhandledException(args.Exception);
        }

        private async Task ShredFileOrFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path should not be null or empty.");

            var operation = new OperationVM { Path = path };
            AddOperation(operation);

            var task = ShredderService.Instance.ShredItemAsync(operation.Path, operation.CancellationTokenSource.Token, operation.TaskProgress);
            try
            {
                var everythingWasShreded = await task;
                if (!everythingWasShreded)
                    OnNotificationRaised(NotificationType.IncompleteFolderShred,
                         "Some files were skipped because they could not be shredded.");
            }
            catch (OperationCanceledException)
            {

            }
            catch (IOException ex)
            {
                OnNotificationRaised(NotificationType.FailedToShredItem, ex.Message);
                LoggerService.Instance.Warning(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                var message = $"Permadelete needs adminstrator's privilages to delete {operation.Path}";
                OnNotificationRaised(NotificationType.FailedToShredItem, message);
                LoggerService.Instance.Warning(ex);
            }
            catch (Exception ex)
            {
                LoggerService.Instance.Error(ex);
                DialogService.Instance.GetErrorDialog("Could not shred item", ex).ShowDialog();
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() => RemoveOperation(operation));
            }
        }

        private void AddOperation(OperationVM operation) => _operationsSource.Add(operation);

        public void RemoveOperation(OperationVM operation) => _operationsSource.Remove(operation);

        private async Task<bool?> GetUserAgreedToDeleteAsync(IEnumerable<string> paths)
        {
            string message;
            string okText = "Shred ";
            var itemName = System.IO.File.Exists(paths.FirstOrDefault()) ? "file" : "folder";

            if (paths.Count() == 1)
            {
                message = $"Are you sure you want to shred this {itemName}?{Environment.NewLine}";
                message += Path.GetFileName(paths.FirstOrDefault());
                okText += "it";
            }
            else
            {
                message = $"Are you sure you want to shred these {paths.Count()} items?";
                okText += "them";
            }
            var dialog = DialogService.Instance.GetMessageDialog("Shredding items", message, MessageIcon.Question, okText, "Cancel", true);
            dialog.Owner = this.MainWindow;
            return await dialog.ShowDialogAsync();
        }

        private void LogUnhandledException(Exception e)
        {
#if !DEBUG
            LoggerService.Instance.Error(e);
            DialogService.Instance.GetErrorDialog("An unxpected error occured", e).ShowDialog();
#endif
        }

        private void OnUpdateStatusChanged()
        {
            UpdateStatusChanged?.Invoke(this, new EventArgs());
        }

        private void OnNotificationRaised(NotificationType type, string message)
        {
            NotificationRaised?.Invoke(this, new NotificationEventArgs(type, message));
        }
        #endregion
    }
}
