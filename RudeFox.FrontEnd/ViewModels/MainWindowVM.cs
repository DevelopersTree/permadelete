using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RudeFox.Mvvm;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using RudeFox.Services;
using Ookii.Dialogs.Wpf;
using System.Collections.Specialized;
using RudeFox.ApplicationManagement;
using System.Windows.Shell;
using System.Windows.Threading;
using System;

namespace RudeFox.ViewModels
{
    class MainWindowVM : BindableBase, IDropTarget
    {
        #region Constructor
        public MainWindowVM()
        {
            ShowAboutCommand = new DelegateCommand(p => DialogService.Instance.GetAboutDialog().ShowDialog());
            ExitCommand = new DelegateCommand(p => Application.Current.Shutdown());

            CancelAllCommand = new DelegateCommand(p =>
            {
                foreach (var operation in App.Operations)
                    operation.CancelCommand.Execute(null);
            }, p => App.Operations.Count > 0);


            DeleteFilesCommand = new DelegateCommand(async p => await DeleteFiles());
            DeleteFoldersCommand = new DelegateCommand(async p => await DeleteFolders());

            (App.Operations as INotifyCollectionChanged).CollectionChanged += Operations_Changed;
            _progressbarTimer.Tick += ProgressbarTimer_Tick;
        }


        #endregion

        #region Fields
        DispatcherTimer _progressbarTimer = new DispatcherTimer();
        long _totalBytes = 0;
        #endregion

        #region Properties
        public string Title { get { return "Rude Fox"; } }

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
        #endregion

        #region Drag and drop
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            var data = dropInfo.Data as IDataObject;
            if (data == null) return;

            if (data.GetDataPresent(DataFormats.FileDrop))
                dropInfo.Effects = DragDropEffects.Move;
            else
                dropInfo.Effects = DragDropEffects.None;
        }

        async void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var data = dropInfo.Data as IDataObject;

            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])data.GetData(DataFormats.FileDrop);
                await App.Instance.DeleteFilesOrFolders(paths.ToList());
            }
        }
        #endregion

        #region Methods
        private async Task DeleteFiles()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "Select the files you want to delete";
            var result = dialog.ShowDialog();
            if (result != true) return;

            var files = dialog.FileNames;
            await App.Instance.DeleteFilesOrFolders(files.ToList());
        }

        private async Task DeleteFolders()
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Select the folder you want to delete";
            dialog.UseDescriptionForTitle = true;
            var result = dialog.ShowDialog();
            if (result != true) return;

            var path = dialog.SelectedPath;
            await App.Instance.DeleteFilesOrFolders(new List<string> { path });
        }

        private void ProgressbarTimer_Tick(object sender, EventArgs e)
        {
            if (App.Operations.Count == 1)
            {
                OverallProgress = App.Operations.First().Progress / 100;
                return;
            }

            var bytesWritten = App.Operations.Sum(o => o.BytesComplete);
            OverallProgress =  (double)bytesWritten / _totalBytes;
        }

        private void Operations_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            CancelAllCommand.RaiseCanExecuteChanged();

            if (App.Operations.Count == 0)
            {
                TaskbarState = TaskbarItemProgressState.None;
                OverallProgress = 0;
                _progressbarTimer.Stop();
            }
            else
            {
                _totalBytes = App.Operations.Sum(o => o.Bytes);

                if (TaskbarState == TaskbarItemProgressState.None)
                    TaskbarState = TaskbarItemProgressState.Normal;
                if (!_progressbarTimer.IsEnabled)
                    _progressbarTimer.Start();
            }
        }
        #endregion
    }
}
