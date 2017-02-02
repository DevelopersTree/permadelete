using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete
{
    public interface IExpireable
    {
        event EventHandler Expired;
        void RaiseExpired();
    }
}
