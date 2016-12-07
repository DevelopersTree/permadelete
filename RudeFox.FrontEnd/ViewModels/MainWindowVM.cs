using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RudeFox.Mvvm;
using System.Windows.Input;
using System.Windows;
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
            ShowAboutCommand = new DelegateCommand(p => DialogService.Instance.GetAboutDialog().ShowDialog());
            ExitCommand = new DelegateCommand(p => Application.Current.Shutdown());
        }
        #endregion

        #region Fields

        #endregion

        #region Properties
        private ObservableCollection<OperationVM> _operations = new ObservableCollection<OperationVM>();
        public ObservableCollection<OperationVM> Operations
        {
            get { return _operations; }
            set { SetProperty(ref _operations, value); }
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
            var response = GetUserAgreedToDelete(paths);
            if (response != true) return;
            
            var duplicates = from i in Operations
                             join p in paths
                             on i.Path equals p
                             select p;

            paths.RemoveAll(p => duplicates.Contains(p));

            var newItems = new List<OperationVM>();
            foreach (var path in paths)
            {
                var item = new OperationVM { Path = path };

                if (File.Exists(path) || Directory.Exists(path))
                    newItems.Add(item);
                else
                    continue;

                Operations.Add(item);
            }

            var tasks = newItems.Select(item => ProcessItem(item)).ToList();

            await Task.WhenAll(tasks);
        }

        private async Task ProcessItem(OperationVM item)
        {
            var task = ShredderService.Instance.ShredItemAsync(item.Path, item.CancellationTokenSource.Token, item.TaskProgress);
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
                Application.Current.Dispatcher.Invoke(() => Operations.Remove(item));
            }
        }

        private bool? GetUserAgreedToDelete(List<string> paths)
        {
            string message;
            string pronoun;
            var itemName = File.Exists(paths[0]) ? "file" : "folder";
            if (paths.Count == 1)
            {
                message = $"Are you sure you want to delete this {itemName}?{Environment.NewLine}";
                message += Path.GetFileName(paths[0]);
                pronoun = "it";
            }
            else
            {
                message = $"Are you sure you want to delete these {paths.Count} items?";
                pronoun = "them";
            }

            return DialogService.Instance.GetMessageDialog("Deleting items", message, MessageIcon.Exclamation, "Delete " + pronoun, "Cancel", true).ShowDialog();
        }
        #endregion
    }
}
