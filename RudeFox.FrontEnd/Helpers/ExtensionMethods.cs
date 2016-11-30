using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudeFox
{
    public static class ExtensionMethods
    {
        public static int RoundOff(this int number, int nearestValue = 5)
        {
            return ((int)Math.Round(number / (double)nearestValue)) * nearestValue;
        }
    }
}
