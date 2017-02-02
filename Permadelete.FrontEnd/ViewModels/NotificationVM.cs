using Permadelete.Enums;
using Permadelete.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.ViewModels
{
    public class NotificationVM : BindableBase, IExpireable
    {
        public NotificationVM(string message, MessageIcon icon = MessageIcon.Information)
        {
            Message = message;
            MessageIcon = icon;
        }

        public MessageIcon MessageIcon { get; private set; }
        public string Icon
        {
            get { return "/Images/" + MessageIcon.ToString().ToLower() + ".png"; }
        }
        public string Message { get; set; }

        public event EventHandler Expired;
        
        public void RaiseExpired()
        {
            Expired?.Invoke(this, new EventArgs());
        }
    }
}
