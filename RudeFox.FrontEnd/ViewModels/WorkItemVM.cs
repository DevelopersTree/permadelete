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
    class WorkItemVM : BindableBase
    {
        #region Constructor
        public WorkItemVM()
        {
            CancelCommand = new DelegateCommand(o =>
            {
                OnDeleteRequested(true);
            });
            CancellationTokenSource = new CancellationTokenSource();
            TaskProgress = new Progress<int>();
            TaskProgress.ProgressChanged += (sender, newBytes) =>
            {
                BytesComplete += newBytes;

                if (Bytes == -1) return;

                Progress = ((double)BytesComplete / Bytes) * 100;
            };
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
                    if (_progress == 100)
                        OnDeleteRequested();
                }
            }
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
                        CalculateBytes();
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

        private Task _task;
        public Task Task
        {
            get { return _task; }
            set { SetProperty(ref _task, value); }
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public Progress<int> TaskProgress { get; set; }

        #endregion

        #region Events
        public event EventHandler<bool> DeleteRequested;
        #endregion

        #region Commands
        public ICommand CancelCommand { get; private set; }
        #endregion

        #region Methods
        private async void CalculateBytes()
        {
            if (File.Exists(Path))
                Bytes = new FileInfo(Path).Length;
            else if (Directory.Exists(Path))
                Bytes = await ShredderService.Instance.GetFolderSize(new DirectoryInfo(Path));
            else
                Bytes = -1;
        }
        private void OnDeleteRequested(bool canceled = false)
        {
            var handler = DeleteRequested;
            handler?.Invoke(this, canceled);
            if (canceled) CancellationTokenSource.Cancel();
        }
        #endregion
    }
}
