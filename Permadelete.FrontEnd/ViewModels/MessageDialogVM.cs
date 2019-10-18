using Permadelete.Enums;
using Permadelete.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Permadelete.ViewModels
{
    class MessageDialogVM : BindableBase
    {
        #region Contructor
        public MessageDialogVM(string title, string message, string okButton) : this(title, message, MessageIcon.Information, okButton, null, false)
        {

        }
        public MessageDialogVM(string title, string message, MessageIcon icon, string okButton, string cancelButton, bool isDestructive)
        {
            Title = title;
            Message = message;
            MessageIcon = icon;

            OkButton = okButton;
            CancelButton = cancelButton;
            IsDestructive = isDestructive;
        }
        #endregion

        #region Properties
        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        private bool _isDestructive;
        public bool IsDestructive
        {
            get { return _isDestructive; }
            set { Set(ref _isDestructive, value); }
        }

        private string _okButton;
        public string OkButton
        {
            get { return _okButton; }
            set { Set(ref _okButton, value); }
        }

        private string _cancelButton;
        public string CancelButton
        {
            get { return _cancelButton; }
            set { Set(ref _cancelButton, value); }
        }

        private MessageIcon _messageIcon;
        public MessageIcon MessageIcon
        {
            get { return _messageIcon; }
            set
            {
                if (Set(ref _messageIcon, value))
                    RaisePropertyChanged(nameof(Icon));
            }
        }

        public string Icon
        {
            get { return "/Images/" + MessageIcon.ToString().ToLower() + ".png"; }
        }
        #endregion
    }
}
