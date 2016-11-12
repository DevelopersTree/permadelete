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
                DeleteItems(paths.ToList());
            }
        }
        #endregion

        #region Methods
        private async void DeleteItems(List<string> paths)
        {
            string message;
            if (paths.Count == 1)
                message = "Are you sure you want to delete this item?";
            else
                message = $"Are you sure you want to delete {paths.Count} items?";

            var response = MessageBox.Show(message, "Deleting items", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (response != MessageBoxResult.Yes) return;

            var newItems = new List<WorkItemVM>();
            var cts = new CancellationTokenSource();

            var duplicates = from i in WorkItems
                             join p in paths
                             on i.Path equals p
                             select p;
            paths.RemoveAll(p => duplicates.Contains(p));

            foreach (var path in paths)
            {
                var item = new WorkItemVM();
                item.Path = path;

                if (File.Exists(path))
                {
                    if (ShredderService.Instance.IsFileLocked(new FileInfo(path)))
                    {
                        MessageBox.Show($"Can't access file, it's being used by another application: {path}");
                        continue;
                    }
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
                    WorkItems.Remove(sender as WorkItemVM);
                    sender = null;
                };

                item.CancellationTokenSource = new CancellationTokenSource();
                WorkItems.Add(item);
            }

            var tasks = newItems.Select(item =>
            {
                return ShredderService.Instance.ShredItemAsync(item.Path, item.CancellationTokenSource.Token, item.TaskProgress);
            });

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {

            }
            catch (AggregateException exc)
            {
                var msg = $"{exc.InnerExceptions.Count()} error(s) occured: ";
                foreach(var ex in exc.InnerExceptions)
                {
                    msg += "\n---------------------------";
                    msg += Environment.NewLine;
                    msg += ex.ToString();
                }
                MessageBox.Show(msg);
            }
            catch( Exception exc)
            {
                var failed = tasks.Where(t => t.IsFaulted);
                MessageBox.Show(exc.ToString());
            }
        }
        #endregion
    }
}
