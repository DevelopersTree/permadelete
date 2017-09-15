using Permadelete.Mvvm;
using Permadelete.UpdateFactory;
using Permadelete.Updater;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Permadelete.UpdateFactory
{
    class MainWindowVM : BindableBase
    {
        #region Constructor
        public MainWindowVM()
        {
            AddCommand = new DelegateCommand(p => Files.Add(new FileVM { Version = "0.0.0.0", IsIncluded = true }));
            DeleteCommand = new DelegateCommand(p => Files.Remove(SelectedFile), p => SelectedFile != null);
        }
        #endregion

        #region Properties
        private bool _indented;
        public bool Indented
        {
            get { return _indented; }
            set { SetProperty(ref _indented, value); }
        }

        private FileVM _selectedFile = null;
        public FileVM SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (SetProperty(ref _selectedFile, value))
                    (DeleteCommand as DelegateCommand).RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<FileVM> _files;
        public ObservableCollection<FileVM> Files
        {
            get { return _files; }
            set { SetProperty(ref _files, value); }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        private string _link;
        public string Link
        {
            get { return _link; }
            set { SetProperty(ref _link, value); }
        }

        private UpdateType _type;
        public UpdateType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private string _path = "data";
        public string Path
        {
            get { return _path; }
            set { SetProperty(ref _path, value); }
        }

        private long _length;
        public long Length
        {
            get { return _length; }
            set { SetProperty(ref _length, value); }
        }

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        #endregion
    }
}
