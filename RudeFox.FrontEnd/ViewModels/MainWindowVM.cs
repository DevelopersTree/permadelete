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

namespace RudeFox.ViewModels
{
    class MainWindowVM : BindableBase, IDropTarget
    {
        #region Constructor
        public MainWindowVM()
        {

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

            var newItems = new List<FileSystemInfo>();

            foreach (var path in paths)
            {
                var item = new WorkItemVM();
                item.Name = path;

                if (File.Exists(path))
                {
                    item.Type = ItemType.File;
                    newItems.Add(new FileInfo(path));
                }
                else if (Directory.Exists(path))
                {
                    item.Type = ItemType.Folder;
                    newItems.Add(new DirectoryInfo(path));
                }
                else return;

                WorkItems.Add(item);
            }

            await ShredderService.Instance.ShredItemsAsync(newItems);
        }
        #endregion
    }
}
