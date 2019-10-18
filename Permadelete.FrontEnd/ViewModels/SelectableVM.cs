using Permadelete.Helpers;
using Permadelete.Mvvm;

namespace Permadelete.ViewModels
{
    public class SelectableVM<T> : BindableBase
    {
        public SelectableVM(T data)
        {
            Data = data;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(ref _isSelected, value); }
        }

        private T _data;
        public T Data
        {
            get { return _data; }
            set { Set(ref _data, value); }
        }
    }
}
