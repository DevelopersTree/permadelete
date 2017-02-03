using Permadelete.Helpers;
using Permadelete.Enums;
using Permadelete.Mvvm;
using Permadelete.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Permadelete.ViewModels
{
    public class OperationVM : BindableBase
    {
        #region Constructor
        public OperationVM()
        {
            CancellationTokenSource = new CancellationTokenSource();
            TaskProgress = new Progress<long>();
            _lastProgressReport = DateTime.Now;

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

        #region Fields
        private double _oldProgress = 0.0;
        private DateTime _lastProgressReport;
        private LinkedList<double> _progressHistory = new LinkedList<double>();
        #endregion

        #region Properties
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                if (SetProperty(ref _path, value))
                {
                    RaisePropertyChanged(nameof(Type));
                    RaisePropertyChanged(nameof(Image));
                    CalculateBytes();
                }
            }
        }

        public ItemType Type
        {
            get
            {
                if (File.Exists(Path))
                    return ItemType.File;
                else if (Directory.Exists(Path))
                    return ItemType.Folder;
                else
                    return ItemType.Unknown;
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            private set
            {
                _oldProgress = _progress;
                if (SetProperty(ref _progress, value))
                {
                    var change = _progress - _oldProgress;
                    var interval = (DateTime.Now - _lastProgressReport).TotalSeconds;
                    var rate = change / interval;
                    _progressHistory.AddFirst(rate);

                    if (_progressHistory.Count > 25)
                        _progressHistory.RemoveLast();

                    var secondsRemaining = (100.0 - _progress) / _progressHistory.Average();
                    TimeRemaining = double.IsNaN(secondsRemaining) ? TimeSpan.FromSeconds(0) : TimeSpan.FromSeconds(secondsRemaining);
                    _lastProgressReport = DateTime.Now;
                }
            }
        }

        private TimeSpan _timeRemaining = TimeSpan.FromSeconds(-1);
        public TimeSpan TimeRemaining
        {
            get { return _timeRemaining; }
            private set { SetProperty(ref _timeRemaining, value); }
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

        public string Image { get { return $"/Images/{Type.ToString().ToLower()}.png"; } }

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
                        return "Calculating...";
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

        public Progress<long> TaskProgress { get; set; }
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
