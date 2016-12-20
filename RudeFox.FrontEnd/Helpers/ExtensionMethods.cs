using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RudeFox
{
    public static class ExtensionMethods
    {
        public static int RoundOff(this int number, int nearestValue = 5)
        {
            return ((int)Math.Round(number / (double)nearestValue)) * nearestValue;
        }

        public async static Task<bool?> ShowDialogAsync(this Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            return await Task.Run(async () =>
            {
                return await window.Dispatcher.InvokeAsync(() => window.ShowDialog());
            });
        }
    }
}
