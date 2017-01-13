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

        public static string ToHumanLanguage(this TimeSpan time)
        {
            if (time >= TimeSpan.FromMinutes(60) && time.Minutes != 0)
                return $"About {time.Hours} hours and {time.Minutes} minutes";
            if (time >= TimeSpan.FromMinutes(50))
                return $"About {time.Hours} hours";
            if (time >= TimeSpan.FromMinutes(30))
                return $"About {time.Minutes} minutes";
            if (time > TimeSpan.FromMinutes(1) && time.Seconds != 0)
                return $"About {time.Minutes} minutes and {time.Seconds.RoundOff()} seconds";
            if (time > TimeSpan.FromMinutes(1))
                return $"About {time.Minutes} minutes";
            if (((int)time.TotalSeconds).RoundOff() > 4)
                return $"About {((int)time.TotalSeconds).RoundOff()} seconds";
            if (((int)time.TotalSeconds).RoundOff() == 0)
                return "Just a moment...";

            return "Calculating...";
        }

        public static string Shorten(this Version version)
        {
            if (version.Revision != 0)
                return version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
            if (version.Build != 0)
                return version.Major + "." + version.Minor + "." + version.Build;

                return version.Major + "." + version.Minor;
        }
    }
}
