using RudeFox.Models;
using RudeFox.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudeFox.ViewModels
{
    class WorkItemVM : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private ItemType _type;
        public ItemType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

    }
}
