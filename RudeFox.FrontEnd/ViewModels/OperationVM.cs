using RudeFox.Helpers;
using RudeFox.Models;
using RudeFox.Mvvm;
using RudeFox.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RudeFox.ViewModels
{
    class OperationVM : BindableBase
    {
        #region Constructor
        public OperationVM()
        {
            CancellationTokenSource = new CancellationTokenSource();
            TaskProgress = new Progress<int>();

            CancelCommand = new DelegateCommand(o =>
            {
                CancellationTokenSource.Cancel();
            });

            TaskProgress.ProgressChanged += (sender, newBytes) =>
            {
                BytesComplete += newBytes;

                if (Bytes != -1)
                    Progress = ((double)BytesComplete / Bytes) * 100;
            };
        }
        #endregion

        #region Properties
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                    CalculateBytes();
            }

        }

        private ItemType _type;
        public ItemType Type
        {
            get
            {
                if (_type == ItemType.NotSet)
                {
                    if (File.Exists(Path))
                        Type = ItemType.File;
                    else if (Directory.Exists(Path))
                        Type = ItemType.Folder;
                    else
                        Type = ItemType.Unknown;
                }

                return _type;
            }
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
            private set { SetProperty(ref _progress, value); }
        }

        private long _bytesComplete;
        public long BytesComplete
        {
            get { return _bytesComplete; }
            set { SetProperty(ref _bytesComplete, value); }
        }

        private long _bytes = -1;
        public long Bytes
        {
            get { return _bytes; }
            set
            {
                if (SetProperty(ref _bytes, value))
                    RaisePropertyChanged(nameof(Size));
            }
        }

        public string Image
        {
            get { return "pack://application:,,,/Images/" + Type.ToString().ToLower() + ".png"; }
        }

        public string Size

        {
            get
            {
                if (Bytes == -1)
                {
                    if (File.Exists(Path))
                        Bytes = new FileInfo(Path).Length;
                    else
                    {
                        return "Calculating";
                    }
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

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public Progress<int> TaskProgress { get; set; }
        #endregion

        #region Commands
        public ICommand CancelCommand { get; private set; }
        #endregion

        #region Methods
        private async void CalculateBytes()
        {
            if (File.Exists(Path))
                _bytes = new FileInfo(Path).Length;
            else if (Directory.Exists(Path))
                _bytes = await ShredderService.Instance.GetFolderSize(new DirectoryInfo(Path));

            RaisePropertyChanged(nameof(Bytes));
            RaisePropertyChanged(nameof(Size));
        }
        #endregion
    }
}
