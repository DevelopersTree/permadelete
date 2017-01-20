using RudeFox.ApplicationManagement;
using RudeFox.Mvvm;
using RudeFox.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

namespace RudeFox.ViewModels
{
    public class AgileWindowVM : BindableBase
    {
        public AgileWindowVM(IEnumerable<string> paths)
        {
            string names;
            string pronoun;
            if (paths.Count() == 1)
            {
                names = $"\"{GetShortName(paths.First())}\"";
                pronoun = string.Empty;
            }
            else
            {
                names = $"{paths.Count()} items";
                pronoun = "these ";
            }

            QuestionTitle = $"Do you want to delete {pronoun}{names}?";
            ProgressTitle = $"Deleting {names}";

            QuestionVisibility = Visibility.Visible;
            TimeRemaining = QuestionTitle;

            TaskbarState = TaskbarItemProgressState.Indeterminate;

            _progressTimer.Interval = TimeSpan.FromMilliseconds(100);
            _progressTimer.Tick += _progressTimer_Tick;

            (App.Operations as INotifyCollectionChanged).CollectionChanged += Operations_Changed;

            CloseCommand = new DelegateCommand(p =>
            {
                if (App.Current.UpdateStatus != Enums.UpdateStatus.DownloadingUpdate)
                {
                    App.Current.Shutdown();
                    return;
                }

                App.Current.MainWindow.Hide();

                App.Current.UpdateStatusChanged += (sender, e) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        if (App.Operations.Count == 0)
                            App.Current.Shutdown();
                        else
                            App.Current?.MainWindow?.Show();
                    });
                };

            });

            DeleteCommand = new DelegateCommand(async p =>
            {
                QuestionVisibility = Visibility.Collapsed;
                TaskbarState = TaskbarItemProgressState.Normal;

                _progressTimer.Start();
                await App.Current.DeleteFilesOrFolders(paths, true);

                CloseCommand.Execute(null);
            });

            OpenRudeFoxCommand = new DelegateCommand(dialog =>
            {
            (App.Operations as INotifyCollectionChanged).CollectionChanged -= Operations_Changed;
            CloseCommand = new DelegateCommand(p => { });

                App.Current.MainWindow = new MainWindow();
                App.Current.MainWindow.Show();
                (dialog as Window).Close();
            });
        }

        #region Fields
        private DispatcherTimer _progressTimer = new DispatcherTimer();
        private TimeSpan _timeToComplete;
        long _totalBytes = 0;
        long _bytesOfCompletedOperations = 0;
        long _writtenBytes = 0;
        #endregion

        #region Commands
        public ICommand CloseCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand OpenRudeFoxCommand { get; set; }
        #endregion

        #region Properties
        public string WindowTitle
        {
            get
            {
                if (QuestionVisibility == Visibility.Visible)
                    return QuestionTitle;
                else
                    return TimeRemaining;
            }
        }

        private string _progressTitle;
        public string ProgressTitle
        {
            get { return _progressTitle; }
            set { SetProperty(ref _progressTitle, value); }
        }

        private string _timeRemaining;
        public string TimeRemaining
        {
            get { return _timeRemaining; }
            set
            {
                if (SetProperty(ref _timeRemaining, value))
                    RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        private TaskbarItemProgressState _taskbarState;
        public TaskbarItemProgressState TaskbarState
        {
            get { return _taskbarState; }
            set { SetProperty(ref _taskbarState, value); }
        }

        private string _questionTitle;
        public string QuestionTitle
        {
            get { return _questionTitle; }
            set
            {
                if (SetProperty(ref _questionTitle, value))
                    RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        private Visibility _questionVisibility;
        public Visibility QuestionVisibility
        {
            get { return _questionVisibility; }
            set { SetProperty(ref _questionVisibility, value); }
        }
        #endregion

        #region Methods
        private void _progressTimer_Tick(object sender, EventArgs e)
        {
            if (App.Operations.Count == 0)
                return;

            _timeToComplete = App.Operations.Max(o => o.TimeRemaining);
            TimeRemaining = _timeToComplete.ToHumanLanguage();

            _writtenBytes = App.Operations.Sum(o => o.BytesComplete);
            Progress = (double)(_writtenBytes + _bytesOfCompletedOperations) / _totalBytes;
        }

        private void Operations_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
                foreach (OperationVM item in e.NewItems)
                    _totalBytes += item.Bytes;

            if (e.OldItems?.Count > 0)
                foreach (OperationVM item in e.OldItems)
                    _bytesOfCompletedOperations += item.Bytes;
        }

        private string GetShortName(string path, int maxLength = 25)
        {
            var name = Path.GetFileName(path);
            if (name.Length > maxLength)
                name = name.Substring(0, maxLength) + "...";
            return name;
        }

        #endregion
    }
}
