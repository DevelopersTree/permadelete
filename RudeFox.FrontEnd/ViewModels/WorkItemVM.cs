using RudeFox.Helpers;
using RudeFox.Models;
using RudeFox.Mvvm;
using RudeFox.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RudeFox.ViewModels
{
    class WorkItemVM : BindableBase
    {
        #region Constructor
        public WorkItemVM()
        {
            CancelCommand = new DelegateCommand(o =>
            {
                OnDeleteRequested(true);
            });
        }
        #endregion

        #region Properties
        private string _path;
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        private ItemType _type;
        public ItemType Type
        {
            get { return _type; }
            set
            {
                if (SetProperty(ref _type, value))
                {
                    RaisePropertyChanged(nameof(Image));
                }
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                if (SetProperty(ref _progress, value))
                {
                    if (_progress == 1.0)
                        OnDeleteRequested();
                }
            }
        }

        private long _length = -1;
        public long Bytes
        {
            get { return _length; }
            set
            {
                if (SetProperty(ref _length, value))
                    RaisePropertyChanged(nameof(Size));
            }
        }

        public string Image
        {
            get { return "pack://application:,,,/images/" + Type.ToString().ToLower() + ".png"; }
        }

        public string Size

        {
            get
            {
                if (Bytes == -1)
                {
                    CalculateBytes();
                    return "Calculating...";
                }
                if (Bytes >= Constants.GIGABYTE)
                {
                    var number = Math.Round(Bytes / (double)Constants.GIGABYTE, 2);
                    return $"{number} GB";
                }
                else if (Bytes >= Constants.MEGABYTE)
                {
                    var number = Math.Round(Bytes / (double)Constants.MEGABYTE, 2);
                    return $"{number} MB";
                }
                else if (Bytes >= Constants.KILOBYTE)
                {
                    var number = Math.Round(Bytes / (double)Constants.KILOBYTE, 2);
                    return $"{number} KB";
                }
                else
                {
                    return $"{Bytes} Bytes";
                }
            }
        }
        #endregion

        #region Events
        public event EventHandler DeleteRequested;
        #endregion

        #region Commands
        public ICommand CancelCommand { get; private set; }
        #endregion

        #region Methods
        private async void CalculateBytes()
        {
            var result = await ShredderService.Instance.GetFolderSize(new System.IO.DirectoryInfo(Path));
            Bytes = result;
        }
        private void OnDeleteRequested(bool canceled = false)
        {
            var handler = DeleteRequested;
            handler?.Invoke(this, new DeleteRequestedEventArgs(canceled));
        }
        #endregion
    }
}
