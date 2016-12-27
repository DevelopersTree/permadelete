using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RudeFox.Xaml
{
    class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan))
                throw new ArgumentException("Value must be a Timespan.");

            TimeSpan time = (TimeSpan)value;

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
