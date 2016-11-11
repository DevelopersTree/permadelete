using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudeFox.Models
{
    class DeleteRequestedEventArgs : EventArgs
    {
        public DeleteRequestedEventArgs(bool canceled)
        {
            Canceled = canceled;
        }
        public bool Canceled { get; private set; }
    }
}
