// based on Sacha Barber's Mediator Pattern implementation
// source: https://www.codeproject.com/Articles/35277/MVVM-Mediator-Pattern
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.Services
{
    public enum NotificationType
    {
        FailedToDeleteItem
    }

    public sealed class NotificationService
    {
        #region Constructor
        private NotificationService()
        {

        }
        #endregion

        #region Properties
        private static readonly Lazy<NotificationService> _instance = new Lazy<NotificationService>(() => new NotificationService());
        public static NotificationService Instance
        {
            get { return _instance.Value; }
        }

        private MultiDictionary<NotificationType, Action<string>> internalList = new MultiDictionary<NotificationType, Action<string>>();
        #endregion

        #region Methods
        public void Register(NotificationType type, Action<string> callback)
        {
            internalList.AddValue(type, callback);
        }

        public void UnRegister(NotificationType type, object instance)
        {
            if (internalList.ContainsKey(type))
                internalList.RemoveAllValue(type, d => d.Target == instance);
        }

        public void BroadcastNotification(NotificationType type, string message)
        {
            if (internalList.ContainsKey(type))
            {
                foreach (var callback in internalList[type])
                    callback(message);
            }
        }
        #endregion
    }
}
