using Permadelete.ApplicationManagement;
using Permadelete.Enums;
using Permadelete.Helpers;
using Permadelete.Mvvm;
using Permadelete.Services;
using Permadelete.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Permadelete.ViewModels
{
    public class QuickWindowVM : BindableBase
    {
        public QuickWindowVM(IEnumerable<string> paths)
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

            QuestionTitle = $"Do you want to shred {pronoun}{names}?";
            ProgressTitle = $"Shredding {names}";

            QuestionVisibility = Visibility.Visible;
            ProgressVisibility = Visibility.Hidden;
            NotificationVisibility = Visibility.Hidden;
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
                ProgressVisibility = Visibility.Visible;

                TaskbarState = TaskbarItemProgressState.Normal;

                _progressTimer.Start();
                await App.Current.DeleteFilesOrFolders(paths, NumberOfPasses);

                if (_hasPendingNotification)
                {
                    ProgressVisibility = Visibility.Collapsed;
                    NotificationVisibility = Visibility.Visible;
                    RaisePropertyChanged(nameof(WindowTitle));
                }
                else
                {
                    CloseCommand.Execute(null);
                }
            });

            OpenMainWindowCommand = new DelegateCommand(dialog =>
            {
                (App.Operations as INotifyCollectionChanged).CollectionChanged -= Operations_Changed;
                CloseCommand = new DelegateCommand(p => { });
                App.Current.NotificationRaised -= OnNotificationRaised;

                App.Current.MainWindow = new MainWindow();
                App.Current.MainWindow.Show();
                (dialog as Window).Close();
            });

            App.Current.NotificationRaised += OnNotificationRaised;

            var settings = SettingsHelper.GetSettings();
            NumberOfPasses = settings.DefaultOverwritePasses;
        }

        #region Fields
        private DispatcherTimer _progressTimer = new DispatcherTimer();
        private TimeSpan _timeToComplete;
        long _totalBytes = 0;
        long _bytesOfCompletedOperations = 0;
        long _writtenBytes = 0;
        private bool _hasPendingNotification = false;
        #endregion

        #region Commands
        public ICommand CloseCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand OpenMainWindowCommand { get; set; }
        #endregion

        #region Properties
        public string WindowTitle
        {
            get
            {
                if (QuestionVisibility == Visibility.Visible)
                    return QuestionTitle;
                else if (ProgressVisibility == Visibility.Visible)
                    return TimeRemaining;
                else
                    return "There was an error in shredding the items.";
            }
        }

        private NotificationVM _notification;
        public NotificationVM Notification
        {
            get { return _notification; }
            set { Set(ref _notification, value); }
        }

        private string _progressTitle;
        public string ProgressTitle
        {
            get { return _progressTitle; }
            set { Set(ref _progressTitle, value); }
        }

        private string _timeRemaining;
        public string TimeRemaining
        {
            get { return _timeRemaining; }
            set
            {
                if (Set(ref _timeRemaining, value))
                    RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        private TaskbarItemProgressState _taskbarState;
        public TaskbarItemProgressState TaskbarState
        {
            get { return _taskbarState; }
            set { Set(ref _taskbarState, value); }
        }

        private string _questionTitle;
        public string QuestionTitle
        {
            get { return _questionTitle; }
            set
            {
                if (Set(ref _questionTitle, value))
                    RaisePropertyChanged(nameof(WindowTitle));
            }
        }

        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set { Set(ref _icon, value); }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set { Set(ref _progress, value); }
        }

        private Visibility _questionVisibility;
        public Visibility QuestionVisibility
        {
            get { return _questionVisibility; }
            set { Set(ref _questionVisibility, value); }
        }

        private Visibility _progressVisibility;
        public Visibility ProgressVisibility
        {
            get { return _progressVisibility; }
            set { Set(ref _progressVisibility, value); }
        }

        private Visibility _notificationVisibility;
        public Visibility NotificationVisibility
        {
            get { return _notificationVisibility; }
            set { Set(ref _notificationVisibility, value); }
        }

        private int _numberOfPasses;
        public int NumberOfPasses
        {
            get { return _numberOfPasses; }
            set { Set(ref _numberOfPasses, value); }
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
                {
                    if (item.Bytes != -1)
                        _totalBytes += item.Bytes;
                    else
                        item.PropertyChanged += DeferAddingToTotalBytes;
                }

            if (e.OldItems?.Count > 0)
                foreach (OperationVM item in e.OldItems)
                {
                    _bytesOfCompletedOperations += item.Bytes;
                    item.PropertyChanged -= DeferAddingToTotalBytes;
                }
        }

        private string GetShortName(string path, int maxLength = 25)
        {
            var name = Path.GetFileName(path);
            if (name.Length > maxLength)
                name = name.Substring(0, maxLength) + "...";
            return name;
        }

        private void RecieveNotification(string message, MessageIcon icon)
        {
            if (Notification != null)
                return;

            Notification = new NotificationVM(message, icon);
            _hasPendingNotification = true;
        }

        private void DeferAddingToTotalBytes(object sender, PropertyChangedEventArgs e)
        {
            var item = (OperationVM)sender;
            if (item != null && e.PropertyName == nameof(item.Bytes) && item.Bytes != -1)
            {
                _totalBytes += item.Bytes;
                item.PropertyChanged -= DeferAddingToTotalBytes;
            }
        }

        private void OnNotificationRaised(object sender, NotificationEventArgs e)
        {
            switch (e.NotificationType)
            {
                case NotificationType.FailedToShredItem:
                    RecieveNotification(e.Message, MessageIcon.Error);
                    break;
                case NotificationType.IncompleteFolderShred:
                    RecieveNotification(e.Message, MessageIcon.Exclamation);
                    break;
            }
        }
        #endregion
    }
}
