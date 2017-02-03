using Permadelete.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete
{
    public enum NotificationType
    {
        FailedToShredItem,
        IncompleteFolderShred
    }

    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(NotificationType type, string message)
        {
            NotificationType = type;
            Message = message;
        }

        public NotificationType NotificationType { get; private set; }
        public string Message { get; private set; }
    }
}
