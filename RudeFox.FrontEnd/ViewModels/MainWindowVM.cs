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
using RudeFox.FrontEnd;
using Ookii.Dialogs.Wpf;
using System.Collections.Specialized;

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

            (App.Operations as INotifyCollectionChanged).CollectionChanged += (sender, e) => CancelAllCommand.RaiseCanExecuteChanged();

            DeleteFilesCommand = new DelegateCommand(async p => await DeleteFiles());
            DeleteFoldersCommand = new DelegateCommand(async p => await DeleteFolders());
        }
        #endregion

        #region Fields

        #endregion

        #region Properties
        public string Title { get { return "Rude Fox"; } }
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
                await DeleteItems(paths.ToList());
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
            await DeleteItems(files.ToList());
        }

        private async Task DeleteFolders()
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Select the folder you want to delete";
            dialog.UseDescriptionForTitle = true;
            var result = dialog.ShowDialog();
            if (result != true) return;

            var path = dialog.SelectedPath;
            await DeleteItems(new List<string> { path });
        }
        private async Task DeleteItems(List<string> paths)
        {
            var userAgreed = await GetUserAgreedToDeleteAsync(paths);
            if (userAgreed != true) return;

            var duplicates = App.Operations.Select(item => item.Path).Intersect(paths);
            paths.RemoveAll(p => duplicates.Contains(p));

            var validPaths = paths.Where(path => File.Exists(path) || Directory.Exists(path));
            var tasks = validPaths.Select(item => App.DeleteFileOrFolder(item)).ToList();

            await Task.WhenAll(tasks);
        }

        private async Task<bool?> GetUserAgreedToDeleteAsync(List<string> paths)
        {
            string message;
            string okText = "Delete ";
            var itemName = File.Exists(paths[0]) ? "file" : "folder";

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
        #endregion
    }
}
