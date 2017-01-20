using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RudeFox.Mvvm;
using System.Windows;
using RudeFox.Services;
using System.Collections.Specialized;
using RudeFox.ApplicationManagement;
using System.Windows.Shell;
using System.Windows.Threading;
using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using RudeFox.Helpers;

namespace RudeFox.ViewModels
{
    class MainWindowVM : BindableBase
    {
        #region Constructor
        public MainWindowVM()
        {
            ShowAboutCommand = new DelegateCommand(parent =>
            {
                var dialog = DialogService.Instance.GetAboutDialog();
                dialog.Owner = (Window)parent;
                dialog.ShowDialog();
            });

            ExitCommand = new DelegateCommand(p => Application.Current.Shutdown());

            CancelAllCommand = new DelegateCommand(p =>
            {
                foreach (var operation in App.Operations)
                    operation.CancelCommand.Execute(null);
            }, p => App.Operations.Count > 0);


            DeleteFilesCommand = new DelegateCommand(async w => await DeleteFiles((Window)w));
            DeleteFoldersCommand = new DelegateCommand(async w => await DeleteFolders((Window)w));

            HandleFileDropCommand = new DelegateCommand(async p =>
            {
                var data = (IDataObject)p;
                if (data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] paths = (string[])data.GetData(DataFormats.FileDrop);
                    await App.Current.DeleteFilesOrFolders(paths);
                }
            });

            (App.Operations as INotifyCollectionChanged).CollectionChanged += Operations_Changed;
            _progressbarTimer.Tick += ProgressbarTimer_Tick;
        }
        #endregion

        #region Fields
        DispatcherTimer _progressbarTimer = new DispatcherTimer();
        long _totalBytes = 0;
        long _bytesOfCompletedOperations = 0;
        long _writtenBytes = 0;
        #endregion

        #region Properties
        public string Title { get { return Constants.APPLICATION_NAME; } }

        private double _overallProgress;
        public double OverallProgress
        {
            get { return _overallProgress; }
            set { SetProperty(ref _overallProgress, value); }
        }

        private TaskbarItemProgressState _taskbarState;
        public TaskbarItemProgressState TaskbarState
        {
            get { return _taskbarState; }
            set { SetProperty(ref _taskbarState, value); }
        }

        #endregion

        #region Commands
        public DelegateCommand ShowAboutCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand CancelAllCommand { get; private set; }
        public DelegateCommand DeleteFilesCommand { get; private set; }
        public DelegateCommand DeleteFoldersCommand { get; private set; }
        public DelegateCommand HandleFileDropCommand { get; set; }
        #endregion

        #region Methods
        private async Task DeleteFiles(Window window)
        {
            var dialog = GetOpenFileDialog();
            dialog.Title = "Select the files you want to delete";

            var result = dialog.ShowDialog(window);
            if (result != CommonFileDialogResult.Ok) return;

            var files = dialog.FileNames;
            await App.Current.DeleteFilesOrFolders(files);
        }

        private async Task DeleteFolders(Window window)
        {
            var dialog = GetOpenFileDialog(true);
            dialog.Title = "Select the folders you want to delete";

            var result = dialog.ShowDialog(window);
            if (result != CommonFileDialogResult.Ok) return;

            var paths = dialog.FileNames;
            await App.Current.DeleteFilesOrFolders(paths);
        }

        private void ProgressbarTimer_Tick(object sender, EventArgs e)
        {
            _writtenBytes = App.Operations.Sum(o => o.BytesComplete);
            OverallProgress = (double)(_writtenBytes + _bytesOfCompletedOperations) / _totalBytes;
        }

        private void Operations_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            CancelAllCommand.RaiseCanExecuteChanged();

            if (App.Operations.Count == 0)
            {
                _totalBytes = 0;
                _writtenBytes = 0;
                _bytesOfCompletedOperations = 0;

                TaskbarState = TaskbarItemProgressState.None;
                OverallProgress = 0;
                _progressbarTimer.Stop();
            }
            else
            {
                if (e.NewItems?.Count > 0)
                    foreach (OperationVM item in e.NewItems)
                        _totalBytes += item.Bytes;

                if (e.OldItems?.Count > 0)
                    foreach (OperationVM item in e.OldItems)
                        _bytesOfCompletedOperations += item.Bytes;

                if (TaskbarState == TaskbarItemProgressState.None)
                    TaskbarState = TaskbarItemProgressState.Normal;
                if (!_progressbarTimer.IsEnabled)
                    _progressbarTimer.Start();
            }
        }

        private CommonOpenFileDialog GetOpenFileDialog(bool isFolderPicker = false)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = true;
            dialog.ShowHiddenItems = true;
            dialog.IsFolderPicker = isFolderPicker;

            return dialog;
        }
        #endregion
    }
}
