using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RudeFox.Mvvm;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.IO;
using RudeFox.Models;
using RudeFox.Services;
using System.Threading;

namespace RudeFox.ViewModels
{
    class MainWindowVM : BindableBase, IDropTarget
    {
        #region Constructor
        public MainWindowVM()
        {
            ShowAboutCommand = new DelegateCommand(p => DialogService.Instance.OpenAboutDialog());
            ExitCommand = new DelegateCommand(p => Application.Current.Shutdown());
        }
        #endregion

        #region Fields

        #endregion

        #region Properties
        private ObservableCollection<WorkItemVM> _workItems = new ObservableCollection<WorkItemVM>();
        public ObservableCollection<WorkItemVM> WorkItems
        {
            get { return _workItems; }
            set { SetProperty(ref _workItems, value); }
        }

        public string Title { get { return "Rude Fox"; } }
        #endregion

        #region Commands
        public ICommand ShowAboutCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
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

        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            var data = dropInfo.Data as IDataObject;

            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])data.GetData(DataFormats.FileDrop);
                DeleteItems(paths);
            }
        }
        #endregion

        #region Methods
        private async void DeleteItems(string[] paths)
        {
            string message;
            if (paths.Length == 1)
                message = "Are you sure you want to delete this item?";
            else
                message = $"Are you sure you want to delete {paths.Length} items?";

            var response = MessageBox.Show(message, "Deleting items", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (response != MessageBoxResult.Yes) return;

            var newItems = new List<WorkItemVM>();
            var cts = new CancellationTokenSource();

            foreach (var path in paths)
            {
                var item = new WorkItemVM();
                item.Path = path;

                if (File.Exists(path))
                {
                    item.Type = ItemType.File;
                    newItems.Add(item);
                }
                else if (Directory.Exists(path))
                {
                    item.Type = ItemType.Folder;
                    newItems.Add(item);
                }
                else return;

                item.DeleteRequested += (sender, canceled) =>
                {
                    if (canceled) cts.Cancel();
                    WorkItems.Remove(sender as WorkItemVM);
                    sender = null;
                };

                WorkItems.Add(item);
            }

            foreach (var item in newItems)
            {
                try
                {
                    var progress = new Progress<double>();
                    progress.ProgressChanged += (sender, percent) =>
                    {
                        item.Progress = percent * 100;
                    };
                    await ShredderService.Instance.ShredItemAsync(item.Path, cts.Token, progress);
                }
                catch (OperationCanceledException)
                {

                }
            }
        }
        #endregion
    }
}
