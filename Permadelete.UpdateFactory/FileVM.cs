using Permadelete.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Permadelete.Updater;

namespace Permadelete.UpdateFactory
{
    public class FileVM : BindableBase
    {
        #region Properties
        private Updater.File _file = new Updater.File();
        public Updater.File File
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }
        
        public string Name
        {
            get { return _file.Name;  }
            set
            {
                _file.Name = value;
                RaisePropertyChanged();
            }
        }

        public string Folder
        {
            get { return _file.Folder; }
            set
            {
                _file.Folder = value;
                RaisePropertyChanged();
            }
        }

        public string Version
        {
            get { return _file.Version.ToString(); }
            set
            {
                _file.Version = new Version(value);
                RaisePropertyChanged();
            }
        }

        public bool Delete
        {
            get { return _file.Delete; }
            set
            {
                _file.Delete = value;
                RaisePropertyChanged();
            }
        }

        public bool Overwrite
        {
            get { return _file.Overwrite; }
            set
            {
                _file.Overwrite = value;
                RaisePropertyChanged();
            }
        }

        public long Length
        {
            get { return _file.Length; }
            set
            {
                _file.Length = value;
                RaisePropertyChanged();
            }
        }

        private FileInfo _fileInfo;
        public FileInfo FileInfo
        {
            get { return _fileInfo; }
            set { SetProperty(ref _fileInfo, value); }
        }

        private bool _isIncluded;
        public bool IsIncluded
        {
            get { return _isIncluded; }
            set { SetProperty(ref _isIncluded, value); }
        }

        #endregion
    }
}
