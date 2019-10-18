using Permadelete.Enums;
using Permadelete.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.ViewModels
{
    class ErrorDialogVM : BindableBase
    {
        #region Contructor
        public ErrorDialogVM(string title, string message, Exception exception)
        {
            Title = title;
            Message = message;
            MessageIcon = MessageIcon.Error;
            Details = GetExceptionInfo(exception);
            OkButton = "Okay";            
        }
        #endregion

        #region Properties
        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private string _details;
        public string Details
        {
            get { return _details; }
            set { Set(ref _details, value); }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set { Set(ref _message, value); }
        }

        private string _okButton;
        public string OkButton
        {
            get { return _okButton; }
            set { Set(ref _okButton, value); }
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

        #region Private Methods
        private string GetExceptionInfo(Exception e)
        {
            string info = string.Empty;
            info += e.ToString();

            if (e.InnerException != null)
            {
                info += "\n\n";
                info += "========== Inner Exception ========= \n";
                info += GetExceptionInfo(e.InnerException);
            }

            return info;
        }
        #endregion
    }
}
